using System.Text.Json.Serialization;
using BackupGenie.ViewModels;

namespace BackupGenie.Models;

public sealed class BackupJob : ViewModelBase
{
    private Guid _id = Guid.NewGuid();
    private string _displayName = string.Empty;
    private string _sourcePath = string.Empty;
    private int _intervalMinutes = 60;
    private bool _isEnabled = true;
    private DateTimeOffset? _lastRunUtc;
    private DateTimeOffset? _lastSuccessUtc;
    private string? _lastError;
    private int _filesScannedLastRun;
    private int _filesCopiedLastRun;
    private bool _isRunning;

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string SourcePath
    {
        get => _sourcePath;
        set => SetProperty(ref _sourcePath, value);
    }

    public int IntervalMinutes
    {
        get => _intervalMinutes;
        set => SetProperty(ref _intervalMinutes, Math.Max(1, value));
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public DateTimeOffset? LastRunUtc
    {
        get => _lastRunUtc;
        set
        {
            if (SetProperty(ref _lastRunUtc, value))
            {
                OnPropertyChanged(nameof(LastRunSummary));
            }
        }
    }

    public DateTimeOffset? LastSuccessUtc
    {
        get => _lastSuccessUtc;
        set
        {
            if (SetProperty(ref _lastSuccessUtc, value))
            {
                OnPropertyChanged(nameof(LastSuccessLocalTime));
                OnPropertyChanged(nameof(LastRunSummary));
            }
        }
    }

    public string? LastError
    {
        get => _lastError;
        set
        {
            if (SetProperty(ref _lastError, value))
            {
                OnPropertyChanged(nameof(LastOutcome));
                OnPropertyChanged(nameof(LastRunSummary));
            }
        }
    }

    public int FilesScannedLastRun
    {
        get => _filesScannedLastRun;
        set
        {
            if (SetProperty(ref _filesScannedLastRun, value))
            {
                OnPropertyChanged(nameof(LastRunSummary));
            }
        }
    }

    public int FilesCopiedLastRun
    {
        get => _filesCopiedLastRun;
        set
        {
            if (SetProperty(ref _filesCopiedLastRun, value))
            {
                OnPropertyChanged(nameof(LastRunSummary));
            }
        }
    }

    [JsonIgnore]
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (SetProperty(ref _isRunning, value))
            {
                OnPropertyChanged(nameof(LastOutcome));
            }
        }
    }

    [JsonIgnore]
    public string LastOutcome => IsRunning
        ? "Running"
        : string.IsNullOrWhiteSpace(LastError) ? "Healthy" : "Needs attention";

    [JsonIgnore]
    public string LastSuccessLocalTime => LastSuccessUtc?.ToLocalTime().ToString("g") ?? "Not yet";

    [JsonIgnore]
    public string LastRunSummary
    {
        get
        {
            if (LastRunUtc is null)
            {
                return "This job has not run yet.";
            }

            if (!string.IsNullOrWhiteSpace(LastError))
            {
                return $"Last run failed at {LastRunUtc.Value.ToLocalTime():g}: {LastError}";
            }

            return $"Last run at {LastRunUtc.Value.ToLocalTime():g}. Scanned {FilesScannedLastRun} file(s), copied {FilesCopiedLastRun}.";
        }
    }
}
