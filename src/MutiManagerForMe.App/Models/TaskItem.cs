namespace MutiManagerForMe.App.Models;

public sealed class TaskItem
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
    public DateTime? ReminderAt { get; set; }
    public int Priority { get; set; } = 2;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string RepeatRule { get; set; } = RepeatRules.None;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string PriorityLabel => Priority switch
    {
        4 => "Khẩn cấp",
        3 => "Cao",
        2 => "Trung bình",
        _ => "Thấp"
    };

    public string DueLabel => DueAt switch
    {
        null => "Không có hạn",
        _ when IsCompleted => "Đã hoàn thành",
        _ when DueAt.Value.Date < DateTime.Today => $"Quá hạn {DueAt:dd/MM}",
        _ when DueAt.Value.Date == DateTime.Today => "Hôm nay",
        _ => DueAt.Value.ToString("dd/MM/yyyy")
    };

    public bool IsOverdue => !IsCompleted && DueAt?.Date < DateTime.Today;
}

public static class RepeatRules
{
    public const string None = "Không lặp";
    public const string Daily = "Hàng ngày";
    public const string Weekly = "Hàng tuần";
    public const string Monthly = "Hàng tháng";

    public static IReadOnlyList<string> All { get; } = [None, Daily, Weekly, Monthly];

    public static DateTime? Next(string rule, DateTime? current) => rule switch
    {
        Daily => (current ?? DateTime.Now).AddDays(1),
        Weekly => (current ?? DateTime.Now).AddDays(7),
        Monthly => (current ?? DateTime.Now).AddMonths(1),
        _ => null
    };
}
