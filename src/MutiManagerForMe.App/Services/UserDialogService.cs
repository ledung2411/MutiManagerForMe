using System.Windows;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace MutiManagerForMe.App.Services;

public interface IUserDialogService
{
    bool Confirm(string message, string title = "Xác nhận");
    void Error(string message, string title = "Không thể thực hiện");
    string? PickBackupPath();
}

public sealed class UserDialogService : IUserDialogService
{
    public bool Confirm(string message, string title = "Xác nhận") =>
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public void Error(string message, string title = "Không thể thực hiện") =>
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public string? PickBackupPath()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Sao lưu dữ liệu",
            Filter = "MutiManager backup (*.db)|*.db",
            FileName = $"MutiManagerForMe-{DateTime.Now:yyyyMMdd-HHmm}.db",
            AddExtension = true,
            DefaultExt = ".db"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
