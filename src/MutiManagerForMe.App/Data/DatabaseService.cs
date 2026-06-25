using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using MutiManagerForMe.App.Models;

namespace MutiManagerForMe.App.Data;

public sealed class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string? databasePath = null)
    {
        var path = databasePath ?? GetDefaultDatabasePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            ForeignKeys = true,
            Pooling = false
        }.ToString();
        DatabasePath = path;
    }

    public string DatabasePath { get; }

    public static string GetDefaultDatabasePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MutiManagerForMe",
        "mutimanager.db");

    public async Task InitializeAsync()
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode = WAL;
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS tasks (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                description TEXT NOT NULL DEFAULT '',
                due_at TEXT NULL,
                reminder_at TEXT NULL,
                reminder_notified INTEGER NOT NULL DEFAULT 0,
                priority INTEGER NOT NULL DEFAULT 2,
                is_completed INTEGER NOT NULL DEFAULT 0,
                completed_at TEXT NULL,
                repeat_rule TEXT NOT NULL DEFAULT 'Không lặp',
                created_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS notes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                content TEXT NOT NULL DEFAULT '',
                is_pinned INTEGER NOT NULL DEFAULT 0,
                updated_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS schedule_entries (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                description TEXT NOT NULL DEFAULT '',
                starts_at TEXT NOT NULL,
                ends_at TEXT NOT NULL,
                reminder_at TEXT NULL,
                reminder_notified INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS wallets (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                initial_balance REAL NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS finance_transactions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                wallet_id INTEGER NOT NULL,
                type TEXT NOT NULL,
                amount REAL NOT NULL,
                category TEXT NOT NULL,
                note TEXT NOT NULL DEFAULT '',
                occurred_at TEXT NOT NULL,
                FOREIGN KEY(wallet_id) REFERENCES wallets(id) ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS budgets (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                month TEXT NOT NULL UNIQUE,
                limit_amount REAL NOT NULL DEFAULT 0
            );

            CREATE INDEX IF NOT EXISTS idx_tasks_due_at ON tasks(due_at);
            CREATE INDEX IF NOT EXISTS idx_tasks_reminder_at ON tasks(reminder_at, reminder_notified);
            CREATE INDEX IF NOT EXISTS idx_schedule_starts_at ON schedule_entries(starts_at);
            CREATE INDEX IF NOT EXISTS idx_schedule_reminder_at ON schedule_entries(reminder_at, reminder_notified);
            CREATE INDEX IF NOT EXISTS idx_transactions_occurred_at ON finance_transactions(occurred_at);
            """;
        await command.ExecuteNonQueryAsync();

        await EnsureColumnAsync(connection, "tasks", "completed_at", "TEXT NULL");
        await EnsureDefaultWalletAsync(connection);
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync(bool includeCompleted = true)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT id, title, description, due_at, reminder_at, priority, is_completed, repeat_rule, created_at, completed_at
            FROM tasks
            {(includeCompleted ? string.Empty : "WHERE is_completed = 0")}
            ORDER BY is_completed ASC,
                     CASE WHEN due_at IS NULL THEN 1 ELSE 0 END,
                     due_at ASC,
                     priority DESC,
                     created_at DESC;
            """;

        var items = new List<TaskItem>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(ReadTask(reader));
        }
        return items;
    }

    public async Task<long> AddTaskAsync(TaskItem item)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO tasks(title, description, due_at, reminder_at, priority, is_completed, completed_at, repeat_rule, created_at)
            VALUES ($title, $description, $dueAt, $reminderAt, $priority, $completed, $completedAt, $repeatRule, $createdAt);
            SELECT last_insert_rowid();
            """;
        AddParameter(command, "$title", item.Title.Trim());
        AddParameter(command, "$description", item.Description.Trim());
        AddParameter(command, "$dueAt", ToDb(item.DueAt));
        AddParameter(command, "$reminderAt", ToDb(item.ReminderAt));
        AddParameter(command, "$priority", item.Priority);
        AddParameter(command, "$completed", item.IsCompleted ? 1 : 0);
        AddParameter(command, "$completedAt", ToDb(item.CompletedAt));
        AddParameter(command, "$repeatRule", item.RepeatRule);
        AddParameter(command, "$createdAt", ToDb(item.CreatedAt));
        return Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public async Task SetTaskCompletedAsync(long id, bool completed)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = "UPDATE tasks SET is_completed = $completed, completed_at = $completedAt WHERE id = $id;";
        AddParameter(command, "$completed", completed ? 1 : 0);
        AddParameter(command, "$completedAt", completed ? ToDb(DateTime.Now) : null);
        AddParameter(command, "$id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteTaskAsync(long id)
    {
        await ExecuteAsync("DELETE FROM tasks WHERE id = $id;", ("$id", id));
    }

    public async Task<IReadOnlyList<NoteEntry>> GetNotesAsync()
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, content, is_pinned, updated_at
            FROM notes
            ORDER BY is_pinned DESC, updated_at DESC;
            """;

        var items = new List<NoteEntry>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new NoteEntry
            {
                Id = reader.GetInt64(0),
                Title = reader.GetString(1),
                Content = reader.GetString(2),
                IsPinned = reader.GetInt64(3) == 1,
                UpdatedAt = FromDb(reader.GetString(4))
            });
        }
        return items;
    }

    public async Task<long> SaveNoteAsync(NoteEntry item)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = item.Id == 0
            ? """
                INSERT INTO notes(title, content, is_pinned, updated_at)
                VALUES ($title, $content, $isPinned, $updatedAt);
                SELECT last_insert_rowid();
                """
            : """
                UPDATE notes SET title = $title, content = $content, is_pinned = $isPinned, updated_at = $updatedAt
                WHERE id = $id;
                SELECT $id;
                """;
        AddParameter(command, "$id", item.Id);
        AddParameter(command, "$title", item.Title.Trim());
        AddParameter(command, "$content", item.Content);
        AddParameter(command, "$isPinned", item.IsPinned ? 1 : 0);
        AddParameter(command, "$updatedAt", ToDb(DateTime.Now));
        return Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public Task DeleteNoteAsync(long id) => ExecuteAsync("DELETE FROM notes WHERE id = $id;", ("$id", id));

    public async Task<IReadOnlyList<ScheduleEntry>> GetScheduleAsync(DateTime? from = null, DateTime? to = null)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, description, starts_at, ends_at, reminder_at
            FROM schedule_entries
            WHERE ($from IS NULL OR starts_at >= $from)
              AND ($to IS NULL OR starts_at < $to)
            ORDER BY starts_at ASC;
            """;
        AddParameter(command, "$from", ToDb(from));
        AddParameter(command, "$to", ToDb(to));

        var items = new List<ScheduleEntry>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new ScheduleEntry
            {
                Id = reader.GetInt64(0),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                StartsAt = FromDb(reader.GetString(3)),
                EndsAt = FromDb(reader.GetString(4)),
                ReminderAt = reader.IsDBNull(5) ? null : FromDb(reader.GetString(5))
            });
        }
        return items;
    }

    public async Task<long> AddScheduleEntryAsync(ScheduleEntry item)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO schedule_entries(title, description, starts_at, ends_at, reminder_at)
            VALUES ($title, $description, $startsAt, $endsAt, $reminderAt);
            SELECT last_insert_rowid();
            """;
        AddParameter(command, "$title", item.Title.Trim());
        AddParameter(command, "$description", item.Description.Trim());
        AddParameter(command, "$startsAt", ToDb(item.StartsAt));
        AddParameter(command, "$endsAt", ToDb(item.EndsAt));
        AddParameter(command, "$reminderAt", ToDb(item.ReminderAt));
        return Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public Task DeleteScheduleEntryAsync(long id) =>
        ExecuteAsync("DELETE FROM schedule_entries WHERE id = $id;", ("$id", id));

    public async Task<IReadOnlyList<Wallet>> GetWalletsAsync()
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT w.id, w.name, w.initial_balance,
                   w.initial_balance + COALESCE(SUM(CASE WHEN t.type = 'Thu' THEN t.amount ELSE -t.amount END), 0) AS balance
            FROM wallets w
            LEFT JOIN finance_transactions t ON t.wallet_id = w.id
            GROUP BY w.id, w.name, w.initial_balance
            ORDER BY w.id;
            """;

        var items = new List<Wallet>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new Wallet
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                InitialBalance = ToDecimal(reader.GetDouble(2)),
                Balance = ToDecimal(reader.GetDouble(3))
            });
        }
        return items;
    }

    public async Task<long> AddWalletAsync(string name, decimal initialBalance)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO wallets(name, initial_balance) VALUES ($name, $balance);
            SELECT last_insert_rowid();
            """;
        AddParameter(command, "$name", name.Trim());
        AddParameter(command, "$balance", (double)initialBalance);
        return Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<FinanceTransaction>> GetTransactionsAsync(DateTime? from = null, DateTime? to = null)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.id, t.wallet_id, w.name, t.type, t.amount, t.category, t.note, t.occurred_at
            FROM finance_transactions t
            INNER JOIN wallets w ON w.id = t.wallet_id
            WHERE ($from IS NULL OR t.occurred_at >= $from)
              AND ($to IS NULL OR t.occurred_at < $to)
            ORDER BY t.occurred_at DESC, t.id DESC;
            """;
        AddParameter(command, "$from", ToDb(from));
        AddParameter(command, "$to", ToDb(to));

        var items = new List<FinanceTransaction>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new FinanceTransaction
            {
                Id = reader.GetInt64(0),
                WalletId = reader.GetInt64(1),
                WalletName = reader.GetString(2),
                Type = reader.GetString(3),
                Amount = ToDecimal(reader.GetDouble(4)),
                Category = reader.GetString(5),
                Note = reader.GetString(6),
                OccurredAt = FromDb(reader.GetString(7))
            });
        }
        return items;
    }

    public async Task<long> AddTransactionAsync(FinanceTransaction item)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO finance_transactions(wallet_id, type, amount, category, note, occurred_at)
            VALUES ($walletId, $type, $amount, $category, $note, $occurredAt);
            SELECT last_insert_rowid();
            """;
        AddParameter(command, "$walletId", item.WalletId);
        AddParameter(command, "$type", item.Type);
        AddParameter(command, "$amount", (double)item.Amount);
        AddParameter(command, "$category", item.Category);
        AddParameter(command, "$note", item.Note.Trim());
        AddParameter(command, "$occurredAt", ToDb(item.OccurredAt));
        return Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public Task DeleteTransactionAsync(long id) =>
        ExecuteAsync("DELETE FROM finance_transactions WHERE id = $id;", ("$id", id));

    public async Task<Budget> GetBudgetAsync(string month)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT id, month, limit_amount FROM budgets WHERE month = $month;";
        AddParameter(command, "$month", month);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return new Budget { Month = month };
        }

        return new Budget
        {
            Id = reader.GetInt64(0),
            Month = reader.GetString(1),
            LimitAmount = ToDecimal(reader.GetDouble(2))
        };
    }

    public async Task SaveBudgetAsync(string month, decimal amount)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO budgets(month, limit_amount) VALUES ($month, $amount)
            ON CONFLICT(month) DO UPDATE SET limit_amount = excluded.limit_amount;
            """;
        AddParameter(command, "$month", month);
        AddParameter(command, "$amount", (double)amount);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<DashboardSnapshot> GetDashboardSnapshotAsync()
    {
        var now = DateTime.Now;
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var allTasks = await GetTasksAsync();
        var todayTasks = allTasks.Where(t => !t.IsCompleted && (t.DueAt?.Date <= today || t.DueAt is null))
                                 .Take(8)
                                 .ToArray();
        var schedule = await GetScheduleAsync(today, tomorrow);
        var notes = (await GetNotesAsync()).Where(n => n.IsPinned).Take(4).ToArray();
        var transactions = await GetTransactionsAsync(monthStart, nextMonth);
        var budget = await GetBudgetAsync(today.ToString("yyyy-MM"));

        return new DashboardSnapshot
        {
            TodayTaskCount = allTasks.Count(t => !t.IsCompleted && t.DueAt?.Date == today),
            CompletedTodayCount = allTasks.Count(t => t.IsCompleted && t.CompletedAt?.Date == today),
            OverdueTaskCount = allTasks.Count(t => t.IsOverdue),
            MonthIncome = transactions.Where(t => !t.IsExpense).Sum(t => t.Amount),
            MonthExpense = transactions.Where(t => t.IsExpense).Sum(t => t.Amount),
            BudgetLimit = budget.LimitAmount,
            TodayTasks = todayTasks,
            TodaySchedule = schedule,
            PinnedNotes = notes
        };
    }

    public async Task<IReadOnlyList<ReminderAlert>> GetDueRemindersAsync(DateTime now)
    {
        await using var connection = await OpenAsync();
        var alerts = new List<ReminderAlert>();
        await using var transaction = await connection.BeginTransactionAsync();

        var query = connection.CreateCommand();
        query.Transaction = (SqliteTransaction)transaction;
        query.CommandText = """
            SELECT 'task' AS source, id, title, reminder_at FROM tasks
            WHERE reminder_at IS NOT NULL AND reminder_at <= $now AND reminder_notified = 0 AND is_completed = 0
            UNION ALL
            SELECT 'schedule' AS source, id, title, reminder_at FROM schedule_entries
            WHERE reminder_at IS NOT NULL AND reminder_at <= $now AND reminder_notified = 0;
            """;
        AddParameter(query, "$now", ToDb(now));
        await using (var reader = await query.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                alerts.Add(new ReminderAlert(
                    reader.GetString(0),
                    reader.GetInt64(1),
                    reader.GetString(2),
                    FromDb(reader.GetString(3))));
            }
        }

        foreach (var alert in alerts)
        {
            var update = connection.CreateCommand();
            update.Transaction = (SqliteTransaction)transaction;
            update.CommandText = alert.Source == "task"
                ? "UPDATE tasks SET reminder_notified = 1 WHERE id = $id;"
                : "UPDATE schedule_entries SET reminder_notified = 1 WHERE id = $id;";
            AddParameter(update, "$id", alert.SourceId);
            await update.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return alerts;
    }

    public async Task CreateBackupAsync(string targetPath)
    {
        await using var source = await OpenAsync();
        var targetConnectionString = new SqliteConnectionStringBuilder { DataSource = targetPath, Pooling = false }.ToString();
        await using var target = new SqliteConnection(targetConnectionString);
        await target.OpenAsync();
        source.BackupDatabase(target);
    }

    private async Task<SqliteConnection> OpenAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static Task EnsureDefaultWalletAsync(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO wallets(name, initial_balance)
            SELECT 'Tiền mặt', 0
            WHERE NOT EXISTS (SELECT 1 FROM wallets);
            """;
        return command.ExecuteNonQueryAsync();
    }

    private static async Task EnsureColumnAsync(SqliteConnection connection, string table, string column, string definition)
    {
        var hasColumn = false;
        {
            var inspect = connection.CreateCommand();
            inspect.CommandText = $"PRAGMA table_info({table});";
            await using var reader = await inspect.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                {
                    hasColumn = true;
                    break;
                }
            }
        }

        if (hasColumn) return;

        var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition};";
        await alter.ExecuteNonQueryAsync();
    }

    private async Task ExecuteAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var connection = await OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var parameter in parameters)
        {
            AddParameter(command, parameter.Name, parameter.Value);
        }
        await command.ExecuteNonQueryAsync();
    }

    private static TaskItem ReadTask(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt64(0),
        Title = reader.GetString(1),
        Description = reader.GetString(2),
        DueAt = reader.IsDBNull(3) ? null : FromDb(reader.GetString(3)),
        ReminderAt = reader.IsDBNull(4) ? null : FromDb(reader.GetString(4)),
        Priority = reader.GetInt32(5),
        IsCompleted = reader.GetInt64(6) == 1,
        RepeatRule = reader.GetString(7),
        CreatedAt = FromDb(reader.GetString(8)),
        CompletedAt = reader.IsDBNull(9) ? null : FromDb(reader.GetString(9))
    };

    private static void AddParameter(SqliteCommand command, string name, object? value)
    {
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    private static string? ToDb(DateTime? value) => value?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static DateTime FromDb(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime();
    private static decimal ToDecimal(double value) => Math.Round(Convert.ToDecimal(value, CultureInfo.InvariantCulture), 2);
}
