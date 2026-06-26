using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutiManagerForMe.App.Data;
using MutiManagerForMe.App.Services;
using System.Diagnostics;
using System.IO;

namespace MutiManagerForMe.App.ViewModels;

public partial class SettingsViewModel(DatabaseService database, IUserDialogService dialogs) : PageViewModel
{
    public override string Title => "Cài đặt";
    public string DatabasePath => database.DatabasePath;
    public string Version => typeof(SettingsViewModel).Assembly.GetName().Version?.ToString(3) ?? "1.1.0";

    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task BackupAsync()
    {
        var target = dialogs.PickBackupPath();
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        try
        {
            await database.CreateBackupAsync(target);
            StatusMessage = $"Đã sao lưu lúc {DateTime.Now:HH:mm}.";
        }
        catch (Exception ex)
        {
            dialogs.Error($"Không thể sao lưu dữ liệu.\n{ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        var folder = Path.GetDirectoryName(DatabasePath);
        if (folder is null)
        {
            return;
        }

        Process.Start(new ProcessStartInfo("explorer.exe", folder) { UseShellExecute = true });
    }
}
