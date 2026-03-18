using System.IO;
using System.Windows.Input;
using BackupGenie.Models;
using BackupGenie.Services;

namespace BackupGenie.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly BackupCoordinator _backupCoordinator;
    private readonly SettingsService _settingsService;
    private readonly StartupRegistrationService _startupRegistrationService;

    private BackupJob? _selectedJob;
    private string _statusMessage;

    public MainViewModel(
        AppSettings settings,
        BackupCoordinator backupCoordinator,
        SettingsService settingsService,
        StartupRegistrationService startupRegistrationService)
    {
        Settings = settings;
        _backupCoordinator = backupCoordinator;
        _settingsService = settingsService;
        _startupRegistrationService = startupRegistrationService;
        _statusMessage = "Choose a central backup folder, add jobs, and save to start scheduling.";

        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        RemoveSelectedJobCommand = new RelayCommand(
            _ => RemoveSelectedJob(),
            _ => SelectedJob is not null);
        RunSelectedNowCommand = new RelayCommand(
            async _ => await RunSelectedNowAsync(),
            _ => SelectedJob is not null);
        RunAllNowCommand = new RelayCommand(async _ => await RunAllNowAsync());
    }

    public AppSettings Settings { get; }

    public BackupJob? SelectedJob
    {
        get => _selectedJob;
        set
        {
            if (SetProperty(ref _selectedJob, value))
            {
                RefreshCommandStates();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SaveCommand { get; }

    public ICommand RemoveSelectedJobCommand { get; }

    public ICommand RunSelectedNowCommand { get; }

    public ICommand RunAllNowCommand { get; }

    public void AddJob(string sourcePath)
    {
        var displayName = Path.GetFileName(sourcePath);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = new DirectoryInfo(sourcePath).Name;
        }

        var job = new BackupJob
        {
            DisplayName = displayName,
            SourcePath = sourcePath,
            IntervalMinutes = 60,
            IsEnabled = true
        };

        Settings.BackupJobs.Add(job);
        SelectedJob = job;
        StatusMessage = $"Added backup job for {displayName}. Save settings to activate scheduling.";
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Settings.CentralBackupLocation))
        {
            StatusMessage = "Choose a central backup location before saving.";
            return;
        }

        if (Settings.BackupJobs.Count == 0)
        {
            StatusMessage = "Add at least one file or folder backup job before saving.";
            return;
        }

        Directory.CreateDirectory(Settings.CentralBackupLocation);
        await _settingsService.SaveAsync(Settings);
        await _startupRegistrationService.ApplyAsync(Settings.LaunchAtSignIn);
        await _backupCoordinator.RestartAsync();
        StatusMessage = "Settings saved. Lightweight background scheduling is now active.";
    }

    private void RemoveSelectedJob()
    {
        if (SelectedJob is null)
        {
            return;
        }

        var removedName = SelectedJob.DisplayName;
        Settings.BackupJobs.Remove(SelectedJob);
        SelectedJob = Settings.BackupJobs.FirstOrDefault();
        StatusMessage = $"Removed backup job {removedName}.";
    }

    private async Task RunSelectedNowAsync()
    {
        if (SelectedJob is null)
        {
            return;
        }

        StatusMessage = $"Running {SelectedJob.DisplayName} now.";
        await _backupCoordinator.RunJobNowAsync(SelectedJob);
        StatusMessage = $"Finished {SelectedJob.DisplayName}.";
    }

    private async Task RunAllNowAsync()
    {
        StatusMessage = "Running all enabled backup jobs.";
        await _backupCoordinator.RunAllNowAsync();
        StatusMessage = "All enabled backup jobs have completed.";
    }

    private void RefreshCommandStates()
    {
        ((RelayCommand)RemoveSelectedJobCommand).RaiseCanExecuteChanged();
        ((RelayCommand)RunSelectedNowCommand).RaiseCanExecuteChanged();
    }
}
