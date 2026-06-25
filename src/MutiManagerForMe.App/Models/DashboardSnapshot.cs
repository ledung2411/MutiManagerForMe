namespace MutiManagerForMe.App.Models;

public sealed class DashboardSnapshot
{
    public int TodayTaskCount { get; set; }
    public int CompletedTodayCount { get; set; }
    public int OverdueTaskCount { get; set; }
    public decimal MonthIncome { get; set; }
    public decimal MonthExpense { get; set; }
    public decimal BudgetLimit { get; set; }
    public IReadOnlyList<TaskItem> TodayTasks { get; set; } = [];
    public IReadOnlyList<ScheduleEntry> TodaySchedule { get; set; } = [];
    public IReadOnlyList<NoteEntry> PinnedNotes { get; set; } = [];

    public decimal BudgetRemaining => BudgetLimit - MonthExpense;
    public double BudgetPercent => BudgetLimit <= 0 ? 0 : Math.Min(100, (double)(MonthExpense / BudgetLimit * 100));
}

public sealed record ReminderAlert(string Source, long SourceId, string Title, DateTime ReminderAt);
