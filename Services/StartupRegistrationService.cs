using Microsoft.Win32;

namespace BackupGenie.Services;

public sealed class StartupRegistrationService
{
    private const string AppName = "BackupGenie";

    public Task ApplyAsync(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run",
            writable: true);

        if (key is null)
        {
            return Task.CompletedTask;
        }

        if (enabled)
        {
            var executablePath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(executablePath))
            {
                key.SetValue(AppName, $"\"{executablePath}\"");
            }
        }
        else
        {
            key.DeleteValue(AppName, false);
        }

        return Task.CompletedTask;
    }
}
