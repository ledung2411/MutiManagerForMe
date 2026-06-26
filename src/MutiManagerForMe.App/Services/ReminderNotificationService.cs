using MutiManagerForMe.App.Data;
using System.Drawing;
using System.Media;
using Forms = System.Windows.Forms;

namespace MutiManagerForMe.App.Services;

public sealed class ReminderNotificationService : IDisposable
{
    private readonly DatabaseService _database;
    private readonly Action _showWindow;
    private readonly Action _exitApplication;
    private readonly Timer _timer;
    private readonly Forms.NotifyIcon _notifyIcon;
    private int _checking;
    private bool _disposed;

    public ReminderNotificationService(DatabaseService database, Action showWindow, Action exitApplication)
    {
        _database = database;
        _showWindow = showWindow;
        _exitApplication = exitApplication;
        _timer = new Timer(OnTimerTick);

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
        _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    public void ShowRunningInTrayMessage()
    {
        _notifyIcon.BalloonTipTitle = "MutiManager vẫn đang chạy";
        _notifyIcon.BalloonTipText = "Nhắc việc vẫn hoạt động. Nhấp đúp biểu tượng để mở lại.";
        _notifyIcon.BalloonTipIcon = Forms.ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(4500);
    }

    private void OnTimerTick(object? state)
    {
        _ = CheckAsync();
    }

    private async Task CheckAsync()
    {
        if (Interlocked.Exchange(ref _checking, 1) == 1)
        {
            return;
        }

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
            Interlocked.Exchange(ref _checking, 0);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _timer.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
