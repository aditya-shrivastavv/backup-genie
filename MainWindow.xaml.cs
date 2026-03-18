using System.ComponentModel;
using System.Windows;
using BackupGenie.ViewModels;
using Forms = System.Windows.Forms;

namespace BackupGenie;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        var app = (App)System.Windows.Application.Current;
        if (!app.IsExitRequested && ViewModel.Settings.RunInBackgroundOnClose)
        {
            e.Cancel = true;
            Hide();
        }
    }

    private void BrowseCentralLocation_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Choose a central folder where all backups will be stored.",
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            ViewModel.Settings.CentralBackupLocation = dialog.SelectedPath;
        }
    }

    private void AddFolderJob_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Choose a folder to back up.",
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            ViewModel.AddJob(dialog.SelectedPath);
        }
    }

    private void AddFileJob_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = true,
            Title = "Choose files to back up"
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (var path in dialog.FileNames)
            {
                ViewModel.AddJob(path);
            }
        }
    }
}
