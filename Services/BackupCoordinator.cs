using System.Collections.Concurrent;
using System.Windows;
using BackupGenie.Models;

namespace BackupGenie.Services;

public sealed class BackupCoordinator
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;
    private readonly BackupRunner _backupRunner;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _jobTokens = new();
    private readonly SemaphoreSlim _backupGate = new(1, 1);

    public BackupCoordinator(SettingsService settingsService, AppSettings settings)
    {
        _settingsService = settingsService;
        _settings = settings;
        _backupRunner = new BackupRunner();
    }

    public async Task StartAsync()
    {
        await RestartAsync();
    }

    public Task StopAsync()
    {
        foreach (var token in _jobTokens.Values)
        {
            token.Cancel();
        }

        _jobTokens.Clear();
        return Task.CompletedTask;
    }

    public async Task RestartAsync()
    {
        await StopAsync();

        foreach (var job in _settings.BackupJobs.Where(job => job.IsEnabled))
        {
            var cts = new CancellationTokenSource();
            _jobTokens[job.Id] = cts;
            _ = Task.Run(() => JobLoopAsync(job, cts.Token), cts.Token);
        }
    }

    public async Task RunAllNowAsync()
    {
        foreach (var job in _settings.BackupJobs.Where(job => job.IsEnabled))
        {
            await RunJobNowAsync(job);
        }
    }

    public async Task RunJobNowAsync(BackupJob job)
    {
        if (string.IsNullOrWhiteSpace(_settings.CentralBackupLocation))
        {
            await UpdateOnUiThreadAsync(() => job.LastError = "Choose a central backup location first.");
            return;
        }

        await _backupGate.WaitAsync();
        try
        {
            await UpdateOnUiThreadAsync(() =>
            {
                job.IsRunning = true;
                job.LastError = null;
            });

            var result = await _backupRunner.RunAsync(job, _settings.CentralBackupLocation);

            await UpdateOnUiThreadAsync(() =>
            {
                var now = DateTimeOffset.UtcNow;
                job.LastRunUtc = now;
                job.LastSuccessUtc = now;
                job.FilesScannedLastRun = result.FilesScanned;
                job.FilesCopiedLastRun = result.FilesCopied;
                job.LastError = null;
                job.IsRunning = false;
            });
        }
        catch (Exception ex)
        {
            await UpdateOnUiThreadAsync(() =>
            {
                job.LastRunUtc = DateTimeOffset.UtcNow;
                job.LastError = ex.Message;
                job.IsRunning = false;
            });
        }
        finally
        {
            _backupGate.Release();
            await _settingsService.SaveAsync(_settings);
        }
    }

    private async Task JobLoopAsync(BackupJob job, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var delay = job.LastRunUtc is null
                    ? TimeSpan.Zero
                    : job.LastRunUtc.Value.AddMinutes(Math.Max(1, job.IntervalMinutes)) - DateTimeOffset.UtcNow;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, token);
                }

                if (!token.IsCancellationRequested && job.IsEnabled)
                {
                    await RunJobNowAsync(job);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static Task UpdateOnUiThreadAsync(Action action)
    {
        return System.Windows.Application.Current.Dispatcher.InvokeAsync(action).Task;
    }
}
