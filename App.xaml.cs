using System.Windows;
using BackupGenie.Services;
using BackupGenie.ViewModels;
using Forms = System.Windows.Forms;

namespace BackupGenie;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _notifyIcon;

    public SettingsService SettingsService { get; private set; } = null!;
    public StartupRegistrationService StartupRegistrationService { get; private set; } = null!;
    public BackupCoordinator BackupCoordinator { get; private set; } = null!;
    public MainViewModel MainViewModel { get; private set; } = null!;
    public bool IsExitRequested { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SettingsService = new SettingsService();
        StartupRegistrationService = new StartupRegistrationService();

        var settings = await SettingsService.LoadAsync();
        await StartupRegistrationService.ApplyAsync(settings.LaunchAtSignIn);

        BackupCoordinator = new BackupCoordinator(SettingsService, settings);
        await BackupCoordinator.StartAsync();

        MainViewModel = new MainViewModel(settings, BackupCoordinator, SettingsService, StartupRegistrationService);
        var window = new MainWindow
        {
            DataContext = MainViewModel
        };

        MainWindow = window;
        CreateTrayIcon();

        if (settings.StartMinimized)
        {
            window.Show();
            window.Hide();
        }
        else
        {
            window.Show();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();

        if (BackupCoordinator is not null)
        {
            await BackupCoordinator.StopAsync();
        }

        base.OnExit(e);
    }

    public void ShowMainWindow()
    {
        if (MainWindow is null)
        {
            return;
        }

        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        MainWindow.Activate();
    }

    public void ExitApplication()
    {
        IsExitRequested = true;
        Shutdown();
    }

    private void CreateTrayIcon()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "Backup Genie",
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Run all backups now", null, async (_, _) => await BackupCoordinator.RunAllNowAsync());
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());
        _notifyIcon.ContextMenuStrip = menu;
    }
}
