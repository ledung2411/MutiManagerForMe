using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Models;

namespace MutiManagerForMe.Tests;

public sealed class DatabaseServiceTests : IAsyncLifetime
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"MutiManagerForMe.Tests-{Guid.NewGuid():N}");
    private DatabaseService _database = null!;

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_directory);
        _database = new DatabaseService(Path.Combine(_directory, "test.db"));
        await _database.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Initialize_CreatesDefaultWallet()
    {
        var wallets = await _database.GetWalletsAsync();

        var wallet = Assert.Single(wallets);
        Assert.Equal("Tiền mặt", wallet.Name);
        Assert.Equal(0, wallet.Balance);
    }

    [Fact]
    public async Task Tasks_ArePersistedAndReminderIsDeliveredOnce()
    {
        var id = await _database.AddTaskAsync(new TaskItem
        {
            Title = "Gửi báo cáo",
            DueAt = DateTime.Today.AddHours(23),
            ReminderAt = DateTime.Now.AddMinutes(-1),
            Priority = 3
        });

        var tasks = await _database.GetTasksAsync();
        var task = Assert.Single(tasks);
        Assert.Equal(id, task.Id);
        Assert.Equal("Gửi báo cáo", task.Title);
        Assert.Equal(3, task.Priority);

        var firstCheck = await _database.GetDueRemindersAsync(DateTime.Now);
        var secondCheck = await _database.GetDueRemindersAsync(DateTime.Now);

        Assert.Single(firstCheck);
        Assert.Empty(secondCheck);

        await _database.SetTaskCompletedAsync(id, true);
        var dashboard = await _database.GetDashboardSnapshotAsync();
        Assert.Equal(1, dashboard.CompletedTodayCount);
    }

    [Fact]
    public async Task FinanceSnapshot_CalculatesBalanceAndBudget()
    {
        var wallet = Assert.Single(await _database.GetWalletsAsync());
        await _database.AddTransactionAsync(new FinanceTransaction
        {
            WalletId = wallet.Id,
            Type = TransactionTypes.Income,
            Amount = 15_000_000,
            Category = "Lương",
            OccurredAt = DateTime.Now
        });
        await _database.AddTransactionAsync(new FinanceTransaction
        {
            WalletId = wallet.Id,
            Type = TransactionTypes.Expense,
            Amount = 2_500_000,
            Category = "Hóa đơn",
            OccurredAt = DateTime.Now
        });
        await _database.SaveBudgetAsync(DateTime.Today.ToString("yyyy-MM"), 8_000_000);

        var snapshot = await _database.GetDashboardSnapshotAsync();
        var updatedWallet = Assert.Single(await _database.GetWalletsAsync());

        Assert.Equal(15_000_000, snapshot.MonthIncome);
        Assert.Equal(2_500_000, snapshot.MonthExpense);
        Assert.Equal(5_500_000, snapshot.BudgetRemaining);
        Assert.Equal(12_500_000, updatedWallet.Balance);
    }

    [Fact]
    public async Task Notes_CanBeCreatedAndUpdated()
    {
        var id = await _database.SaveNoteAsync(new NoteEntry
        {
            Title = "Họp tuần",
            Content = "Nội dung ban đầu",
            IsPinned = true
        });
        await _database.SaveNoteAsync(new NoteEntry
        {
            Id = id,
            Title = "Họp tuần",
            Content = "Nội dung đã sửa",
            IsPinned = false
        });

        var note = Assert.Single(await _database.GetNotesAsync());
        Assert.Equal("Nội dung đã sửa", note.Content);
        Assert.False(note.IsPinned);
    }

    [Fact]
    public async Task Backup_CreatesReadableDatabaseCopy()
    {
        await _database.AddTaskAsync(new TaskItem { Title = "Dữ liệu cần sao lưu" });
        var backupPath = Path.Combine(_directory, "backup.db");

        await _database.CreateBackupAsync(backupPath);
        var backup = new DatabaseService(backupPath);
        await backup.InitializeAsync();

        var task = Assert.Single(await backup.GetTasksAsync());
        Assert.Equal("Dữ liệu cần sao lưu", task.Title);
    }
}
