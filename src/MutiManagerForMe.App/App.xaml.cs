using System.Globalization;
using System.Windows;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Services;
using MutiManagerForMe.App.ViewModels;

namespace MutiManagerForMe.App;

public partial class App : System.Windows.Application
{
    private ReminderNotificationService? _reminderService;
    private MainWindow? _mainWindow;
    private bool _isExiting;
    private bool _trayMessageShown;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");

        try
        {
            var database = new DatabaseService();
            await database.InitializeAsync();
            var dialogs = new UserDialogService();

            _mainWindow = new MainWindow
            {
                DataContext = new MainViewModel(database, dialogs)
            };
            _mainWindow.Closing += OnMainWindowClosing;

            _reminderService = new ReminderNotificationService(database, ShowMainWindow, ExitApplication);
            _reminderService.Start();
            _mainWindow.Show();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Không thể khởi động MutiManagerForMe.\n\n{ex.Message}",
                "Lỗi khởi động",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isExiting) return;
        e.Cancel = true;
        _mainWindow?.Hide();
        if (!_trayMessageShown)
        {
            _trayMessageShown = true;
            _reminderService?.ShowRunningInTrayMessage();
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null) return;
        _mainWindow.Show();
        if (_mainWindow.WindowState == WindowState.Minimized)
        {
            _mainWindow.WindowState = WindowState.Normal;
        }
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        _reminderService?.Dispose();
        _mainWindow?.Close();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _reminderService?.Dispose();
        base.OnExit(e);
    }
}
