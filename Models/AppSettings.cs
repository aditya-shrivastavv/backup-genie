using System.Collections.ObjectModel;
using BackupGenie.ViewModels;

namespace BackupGenie.Models;

public sealed class AppSettings : ViewModelBase
{
    private string _centralBackupLocation = string.Empty;
    private bool _runInBackgroundOnClose = true;
    private bool _startMinimized;
    private bool _launchAtSignIn;

    public string CentralBackupLocation
    {
        get => _centralBackupLocation;
        set => SetProperty(ref _centralBackupLocation, value);
    }

    public bool RunInBackgroundOnClose
    {
        get => _runInBackgroundOnClose;
        set => SetProperty(ref _runInBackgroundOnClose, value);
    }

    public bool StartMinimized
    {
        get => _startMinimized;
        set => SetProperty(ref _startMinimized, value);
    }

    public bool LaunchAtSignIn
    {
        get => _launchAtSignIn;
        set => SetProperty(ref _launchAtSignIn, value);
    }

    public ObservableCollection<BackupJob> BackupJobs { get; set; } = [];
}
