# Backup Genie

Backup Genie is a Windows 11 desktop app for lightweight scheduled backups of user-selected files and folders into one central backup location.

## What this build includes

- Native Windows GUI built with WPF.
- Configurable central backup destination.
- Add file or folder backup jobs directly from the UI.
- Individual backup interval per job in minutes.
- Background-friendly scheduling with no busy polling.
- System tray behavior so the app can stay running without keeping the window open.
- Optional auto-start at Windows sign-in.
- Incremental copy behavior that skips unchanged files.

## How it is optimized

The scheduler runs one lightweight loop per enabled job and sleeps between runs instead of continuously scanning the filesystem. Actual copy work is serialized through a gate so the app does not stampede the disk with overlapping backup jobs.

## Project structure

- `App.xaml` and `App.xaml.cs`: application startup and tray integration.
- `MainWindow.xaml` and `MainWindow.xaml.cs`: Windows 11 desktop UI.
- `Models`: backup job and app settings models.
- `Services`: settings persistence, scheduling, file copy engine, and startup registration.
- `ViewModels`: UI state and commands.

## Prerequisites

Install one of the following on Windows:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or later with the `.NET desktop development` workload

## Building

Open PowerShell in the project folder:

```powershell
cd D:\projects\backup-genie
```

Build the app:

```powershell
dotnet build
```

If `dotnet` is not available in your current shell, use the full path:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" build
```

Successful build output is created under:

```text
bin\Debug\net10.0-windows\
```

## Running

Run the app directly from source:

```powershell
dotnet run
```

Or with the full executable path to `dotnet`:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" run
```

You can also run the built executable after a successful build:

```powershell
& "D:\projects\backup-genie\bin\Debug\net10.0-windows\BackupGenie.exe"
```

## Publishing

Create a Release publish output:

```powershell
dotnet publish -c Release
```

Or:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" publish -c Release
```

Published files are generated in:

```text
bin\Release\net10.0-windows\publish\
```

The published app can be started with:

```powershell
& "D:\projects\backup-genie\bin\Release\net10.0-windows\publish\BackupGenie.exe"
```

## Notes

- Settings are stored in `%LOCALAPPDATA%\BackupGenie\settings.json`.
- Each backup job gets its own folder beneath the configured central backup location.
- This implementation mirrors current files and skips unchanged files, but it does not yet provide version history, compression, encryption, or deleted-file retention.

For a fuller build, run, publish, packaging, and screenshot guide, see [BUILDING.md](D:\projects\backup-genie\BUILDING.md).
