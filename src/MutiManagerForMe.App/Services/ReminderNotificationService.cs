using MutiManagerForMe.App.Data;
using System.Drawing;
using System.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace MutiManagerForMe.App.Services;

public sealed class ReminderNotificationService : IDisposable
{
    private readonly DatabaseService _database;
    private readonly Action _showWindow;
    private readonly Action _exitApplication;
    private readonly DispatcherTimer _timer;
    private readonly Forms.NotifyIcon _notifyIcon;
    private bool _checking;

    public ReminderNotificationService(DatabaseService database, Action showWindow, Action exitApplication)
    {
        _database = database;
        _showWindow = showWindow;
        _exitApplication = exitApplication;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _timer.Tick += OnTimerTick;

        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add("Mở MutiManager", null, (_, _) => _showWindow());
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add("Thoát", null, (_, _) => _exitApplication());

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "MutiManagerForMe",
            Visible = true,
            ContextMenuStrip = contextMenu
        };
        _notifyIcon.DoubleClick += (_, _) => _showWindow();
    }

    public void Start()
    {
        _timer.Start();
        _ = CheckAsync();
    }

    public void ShowRunningInTrayMessage()
    {
        _notifyIcon.BalloonTipTitle = "MutiManager vẫn đang chạy";
        _notifyIcon.BalloonTipText = "Nhắc việc vẫn hoạt động. Nhấp đúp biểu tượng để mở lại.";
        _notifyIcon.BalloonTipIcon = Forms.ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(4500);
    }

    private async void OnTimerTick(object? sender, EventArgs e) => await CheckAsync();

    private async Task CheckAsync()
    {
        if (_checking) return;
        _checking = true;
        try
        {
            var alerts = await _database.GetDueRemindersAsync(DateTime.Now);
            foreach (var alert in alerts)
            {
                SystemSounds.Exclamation.Play();
                _notifyIcon.BalloonTipTitle = alert.Source == "task" ? "Nhắc công việc" : "Nhắc lịch trình";
                _notifyIcon.BalloonTipText = $"{alert.Title}\nĐến giờ lúc {alert.ReminderAt:HH:mm dd/MM}.";
                _notifyIcon.BalloonTipIcon = Forms.ToolTipIcon.Info;
                _notifyIcon.ShowBalloonTip(8000);
            }
        }
        catch
        {
            // A transient database error should not stop the reminder loop.
        }
        finally
        {
            _checking = false;
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
