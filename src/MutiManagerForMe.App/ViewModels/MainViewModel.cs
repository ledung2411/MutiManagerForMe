using CommunityToolkit.Mvvm.ComponentModel;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Services;
using System.Collections.ObjectModel;

namespace MutiManagerForMe.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IUserDialogService _dialogs;

    public MainViewModel(DatabaseService database, IUserDialogService dialogs)
    {
        _dialogs = dialogs;
        NavigationItems =
        [
            new("Tổng quan", "⌂", new DashboardViewModel(database)),
            new("Công việc", "✓", new TasksViewModel(database, dialogs)),
            new("Ghi chú", "▤", new NotesViewModel(database, dialogs)),
            new("Lịch trình", "▦", new ScheduleViewModel(database, dialogs)),
            new("Thu chi", "₫", new FinanceViewModel(database, dialogs)),
            new("Cài đặt", "⚙", new SettingsViewModel(database, dialogs))
        ];
        SelectedNavigationItem = NavigationItems[0];
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; }

    [ObservableProperty] private NavigationItem? selectedNavigationItem;
    [ObservableProperty] private PageViewModel? currentPage;

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value is null)
        {
            return;
        }

        CurrentPage = value.Page;
        _ = LoadPageSafeAsync(value.Page);
    }

    private async Task LoadPageSafeAsync(PageViewModel page)
    {
        try
        {
            await page.LoadAsync();
        }
        catch (Exception ex)
        {
            _dialogs.Error($"Không thể tải dữ liệu.\n{ex.Message}", "MutiManagerForMe");
        }
    }
}
