using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Services;
using MutiManagerForMe.App.ViewModels;
using Windows.Graphics;
using WinRT.Interop;

namespace MutiManagerForMe.App;

public partial class App : Application
{
    private const int SwHide = 0;
    private const int SwShow = 5;

    private ReminderNotificationService? _reminderService;
    private MainWindow? _mainWindow;
    private IntPtr _mainWindowHandle;
    private bool _isExiting;
    private bool _trayMessageShown;

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");

        try
        {
            var database = new DatabaseService();
            await database.InitializeAsync();

            var dialogs = new UserDialogService();
            _mainWindow = new MainWindow();
            _mainWindow.SetViewModel(new MainViewModel(database, dialogs));
            _mainWindow.AppWindow.Resize(new SizeInt32(1280, 820));
            _mainWindow.AppWindow.Closing += OnMainWindowClosing;
            _mainWindowHandle = WindowNative.GetWindowHandle(_mainWindow);

            _reminderService = new ReminderNotificationService(database, ShowMainWindow, ExitApplication);
            _reminderService.Start();
            _mainWindow.Activate();
        }
        catch (Exception ex)
        {
            UserDialogService.ErrorMessage(
                $"Không thể khởi động MutiManagerForMe.\n\n{ex.Message}",
                "Lỗi khởi động");
            Exit();
        }
    }

    private void OnMainWindowClosing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (_isExiting)
        {
            return;
        }

        args.Cancel = true;
        HideMainWindow();
        if (!_trayMessageShown)
        {
            _trayMessageShown = true;
            _reminderService?.ShowRunningInTrayMessage();
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        if (_mainWindowHandle != IntPtr.Zero)
        {
            ShowWindow(_mainWindowHandle, SwShow);
            SetForegroundWindow(_mainWindowHandle);
        }

        _mainWindow.Activate();
    }

    private void HideMainWindow()
    {
        if (_mainWindowHandle != IntPtr.Zero)
        {
            ShowWindow(_mainWindowHandle, SwHide);
        }
    }

    private void ExitApplication()
    {
        _isExiting = true;
        _reminderService?.Dispose();
        _mainWindow?.Close();
        Exit();
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
