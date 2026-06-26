using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MutiManagerForMe.App.ViewModels;
using MutiManagerForMe.App.Views;

namespace MutiManagerForMe.App;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private bool _syncingSelection;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void SetViewModel(MainViewModel viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Root.DataContext = viewModel;
        NavigationList.ItemsSource = viewModel.NavigationItems;
        NavigationList.SelectedItem = viewModel.SelectedNavigationItem;
        ShowPage(viewModel.CurrentPage);
    }

    private void NavigationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingSelection || _viewModel is null || NavigationList.SelectedItem is not NavigationItem item)
        {
            return;
        }

        _viewModel.SelectedNavigationItem = item;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (e.PropertyName is nameof(MainViewModel.SelectedNavigationItem))
        {
            _syncingSelection = true;
            NavigationList.SelectedItem = _viewModel.SelectedNavigationItem;
            _syncingSelection = false;
        }

        if (e.PropertyName is nameof(MainViewModel.CurrentPage) or nameof(MainViewModel.SelectedNavigationItem))
        {
            ShowPage(_viewModel.CurrentPage);
        }
    }

    private void ShowPage(PageViewModel? page)
    {
        if (page is null)
        {
            PageHost.Content = null;
            return;
        }

        UserControl view = page switch
        {
            DashboardViewModel => new DashboardView(),
            TasksViewModel => new TasksView(),
            NotesViewModel => new NotesView(),
            ScheduleViewModel => new ScheduleView(),
            FinanceViewModel => new FinanceView(),
            SettingsViewModel => new SettingsView(),
            _ => new DashboardView()
        };

        view.DataContext = page;
        PageHost.Content = view;
    }
}
