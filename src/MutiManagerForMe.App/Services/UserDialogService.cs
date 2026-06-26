using Forms = System.Windows.Forms;

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
        Forms.MessageBox.Show(
            message,
            title,
            Forms.MessageBoxButtons.YesNo,
            Forms.MessageBoxIcon.Question) == Forms.DialogResult.Yes;

    public void Error(string message, string title = "Không thể thực hiện") =>
        ErrorMessage(message, title);

    public static void ErrorMessage(string message, string title = "Không thể thực hiện") =>
        Forms.MessageBox.Show(
            message,
            title,
            Forms.MessageBoxButtons.OK,
            Forms.MessageBoxIcon.Warning);

    public string? PickBackupPath()
    {
        using var dialog = new Forms.SaveFileDialog
        {
            Title = "Sao lưu dữ liệu",
            Filter = "MutiManager backup (*.db)|*.db",
            FileName = $"MutiManagerForMe-{DateTime.Now:yyyyMMdd-HHmm}.db",
            AddExtension = true,
            DefaultExt = ".db"
        };

        return dialog.ShowDialog() == Forms.DialogResult.OK ? dialog.FileName : null;
    }
}
