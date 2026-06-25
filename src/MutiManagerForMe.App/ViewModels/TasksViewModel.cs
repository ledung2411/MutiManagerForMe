using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Models;
using MutiManagerForMe.App.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MutiManagerForMe.App.ViewModels;

public partial class TasksViewModel(DatabaseService database, IUserDialogService dialogs) : PageViewModel
{
    public override string Title => "Công việc";

    public ObservableCollection<TaskItem> Items { get; } = [];
    public IReadOnlyList<PriorityChoice> Priorities { get; } =
        [new(1, "Thấp"), new(2, "Trung bình"), new(3, "Cao"), new(4, "Khẩn cấp")];
    public IReadOnlyList<string> RepeatOptions => RepeatRules.All;

    [ObservableProperty] private string newTitle = string.Empty;
    [ObservableProperty] private string newDescription = string.Empty;
    [ObservableProperty] private DateTime? dueDate = DateTime.Today;
    [ObservableProperty] private DateTime? reminderDate;
    [ObservableProperty] private string reminderTime = "09:00";
    [ObservableProperty] private PriorityChoice? selectedPriority;
    [ObservableProperty] private string selectedRepeat = RepeatRules.None;
    [ObservableProperty] private bool showCompleted = true;
    [ObservableProperty] private string validationMessage = string.Empty;

    public override async Task LoadAsync()
    {
        SelectedPriority ??= Priorities[1];
        var items = await database.GetTasksAsync(ShowCompleted);
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    partial void OnShowCompletedChanged(bool value) => _ = LoadAsync();

    [RelayCommand]
    private async Task AddAsync()
    {
        ValidationMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            ValidationMessage = "Nhập tên công việc.";
            return;
        }

        DateTime? reminder = null;
        if (ReminderDate is not null)
        {
            if (!TimeSpan.TryParseExact(ReminderTime.Trim(), ["h\\:mm", "hh\\:mm"], CultureInfo.InvariantCulture, out var time))
            {
                ValidationMessage = "Giờ nhắc cần có dạng HH:mm.";
                return;
            }
            reminder = ReminderDate.Value.Date.Add(time);
        }

        await database.AddTaskAsync(new TaskItem
        {
            Title = NewTitle,
            Description = NewDescription,
            DueAt = DueDate?.Date.AddHours(23).AddMinutes(59),
            ReminderAt = reminder,
            Priority = SelectedPriority?.Value ?? 2,
            RepeatRule = SelectedRepeat
        });

        NewTitle = string.Empty;
        NewDescription = string.Empty;
        ReminderDate = null;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ToggleCompletedAsync(TaskItem? item)
    {
        if (item is null) return;
        var completed = !item.IsCompleted;
        await database.SetTaskCompletedAsync(item.Id, completed);

        if (completed && item.RepeatRule != RepeatRules.None)
        {
            var nextDue = RepeatRules.Next(item.RepeatRule, item.DueAt);
            var nextReminder = RepeatRules.Next(item.RepeatRule, item.ReminderAt);
            await database.AddTaskAsync(new TaskItem
            {
                Title = item.Title,
                Description = item.Description,
                DueAt = nextDue,
                ReminderAt = nextReminder,
                Priority = item.Priority,
                RepeatRule = item.RepeatRule
            });
        }

        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync(TaskItem? item)
    {
        if (item is null || !dialogs.Confirm($"Xóa công việc “{item.Title}”?")) return;
        await database.DeleteTaskAsync(item.Id);
        await LoadAsync();
    }
}
