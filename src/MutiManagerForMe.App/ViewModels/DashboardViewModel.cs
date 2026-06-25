using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Models;
using System.Collections.ObjectModel;

namespace MutiManagerForMe.App.ViewModels;

public partial class DashboardViewModel(DatabaseService database) : PageViewModel
{
    public override string Title => "Tổng quan";

    public ObservableCollection<TaskItem> TodayTasks { get; } = [];
    public ObservableCollection<ScheduleEntry> TodaySchedule { get; } = [];
    public ObservableCollection<NoteEntry> PinnedNotes { get; } = [];

    [ObservableProperty] private int todayTaskCount;
    [ObservableProperty] private int completedTodayCount;
    [ObservableProperty] private int overdueTaskCount;
    [ObservableProperty] private decimal monthIncome;
    [ObservableProperty] private decimal monthExpense;
    [ObservableProperty] private decimal budgetRemaining;
    [ObservableProperty] private double budgetPercent;
    [ObservableProperty] private bool isLoading;

    public string Greeting
    {
        get
        {
            var hour = DateTime.Now.Hour;
            return hour < 11 ? "Chào buổi sáng" : hour < 18 ? "Chào buổi chiều" : "Chào buổi tối";
        }
    }

    public string TodayLabel => DateTime.Now.ToString("dddd, dd 'tháng' MM", new System.Globalization.CultureInfo("vi-VN"));

    public override async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var snapshot = await database.GetDashboardSnapshotAsync();
            TodayTaskCount = snapshot.TodayTaskCount;
            CompletedTodayCount = snapshot.CompletedTodayCount;
            OverdueTaskCount = snapshot.OverdueTaskCount;
            MonthIncome = snapshot.MonthIncome;
            MonthExpense = snapshot.MonthExpense;
            BudgetRemaining = snapshot.BudgetRemaining;
            BudgetPercent = snapshot.BudgetPercent;
            Replace(TodayTasks, snapshot.TodayTasks);
            Replace(TodaySchedule, snapshot.TodaySchedule);
            Replace(PinnedNotes, snapshot.PinnedNotes);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }
}
