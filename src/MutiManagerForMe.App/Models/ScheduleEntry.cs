namespace MutiManagerForMe.App.Models;

public sealed class ScheduleEntry
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; } = DateTime.Now;
    public DateTime EndsAt { get; set; } = DateTime.Now.AddHours(1);
    public DateTime? ReminderAt { get; set; }

    public string TimeLabel => $"{StartsAt:HH:mm} – {EndsAt:HH:mm}";
    public string DateLabel => StartsAt.Date == DateTime.Today ? "Hôm nay" : StartsAt.ToString("dd/MM/yyyy");
}
