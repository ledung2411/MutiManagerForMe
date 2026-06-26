using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Models;
using MutiManagerForMe.App.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MutiManagerForMe.App.ViewModels;

public partial class ScheduleViewModel(DatabaseService database, IUserDialogService dialogs) : PageViewModel
{
    public override string Title => "Lịch trình";
    public ObservableCollection<ScheduleEntry> Items { get; } = [];

    [ObservableProperty] private DateTimeOffset selectedDate = DateTimeOffset.Now;
    [ObservableProperty] private string newTitle = string.Empty;
    [ObservableProperty] private string newDescription = string.Empty;
    [ObservableProperty] private string startTime = "09:00";
    [ObservableProperty] private string endTime = "10:00";
    [ObservableProperty] private bool enableReminder = true;
    [ObservableProperty] private int reminderMinutes = 15;
    [ObservableProperty] private string validationMessage = string.Empty;

    public string SelectedDateLabel => SelectedDate.DateTime.ToString("dddd, dd/MM/yyyy", new CultureInfo("vi-VN"));

    public override async Task LoadAsync()
    {
        var day = SelectedDate.DateTime.Date;
        var entries = await database.GetScheduleAsync(day, day.AddDays(1));
        Items.Clear();
        foreach (var entry in entries)
        {
            Items.Add(entry);
        }
        OnPropertyChanged(nameof(SelectedDateLabel));
    }

    partial void OnSelectedDateChanged(DateTimeOffset value) => _ = LoadAsync();

    [RelayCommand]
    private async Task AddAsync()
    {
        ValidationMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            ValidationMessage = "Nhập tên lịch trình.";
            return;
        }

        if (!TryTime(StartTime, out var start) || !TryTime(EndTime, out var end))
        {
            ValidationMessage = "Thời gian cần có dạng HH:mm.";
            return;
        }

        var day = SelectedDate.DateTime.Date;
        var startsAt = day.Add(start);
        var endsAt = day.Add(end);
        if (endsAt <= startsAt)
        {
            ValidationMessage = "Giờ kết thúc phải sau giờ bắt đầu.";
            return;
        }

        await database.AddScheduleEntryAsync(new ScheduleEntry
        {
            Title = NewTitle,
            Description = NewDescription,
            StartsAt = startsAt,
            EndsAt = endsAt,
            ReminderAt = EnableReminder ? startsAt.AddMinutes(-Math.Max(0, ReminderMinutes)) : null
        });

        NewTitle = string.Empty;
        NewDescription = string.Empty;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync(ScheduleEntry? item)
    {
        if (item is null || !dialogs.Confirm($"Xóa lịch “{item.Title}”?"))
        {
            return;
        }

        await database.DeleteScheduleEntryAsync(item.Id);
        await LoadAsync();
    }

    [RelayCommand]
    private void PreviousDay() => SelectedDate = SelectedDate.AddDays(-1);

    [RelayCommand]
    private void NextDay() => SelectedDate = SelectedDate.AddDays(1);

    [RelayCommand]
    private void Today() => SelectedDate = DateTimeOffset.Now;

    private static bool TryTime(string text, out TimeSpan value) =>
        TimeSpan.TryParseExact(text.Trim(), ["h\\:mm", "hh\\:mm"], CultureInfo.InvariantCulture, out value);
}
