# Building And Packaging Backup Genie

This guide explains how to build, run, publish, package, and share Backup Genie with other Windows users.

## Audience

Use this document if you want to:

- build the app locally for development
- run the app from source
- publish a Release build
- package the published output for other people
- capture screenshots for documentation or release notes

## Prerequisites

Install one of the following on Windows 11:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or later with the `.NET desktop development` workload

Optional tools for packaging:

- 7-Zip for creating `.zip` archives
- Inno Setup if you want to create a Windows installer

## Project Location

All commands below assume the project is located at:

```text
D:\projects\backup-genie
```

Open PowerShell and move into the project:

```powershell
cd D:\projects\backup-genie
```

If `dotnet` is not available in your current terminal session, use the full executable path:

```powershell
& "C:\Program Files\dotnet\dotnet.exe"
```

## Build For Development

Build the project:

```powershell
dotnet build
```

Or:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" build
```

Expected output:

```text
bin\Debug\net10.0-windows\
```

Important files after a successful build:

- `BackupGenie.exe`
- `BackupGenie.dll`
- `BackupGenie.runtimeconfig.json`
- `BackupGenie.deps.json`

## Run The App

Run directly from source:

```powershell
dotnet run
```

Or:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" run
```

Run the built executable directly:

```powershell
& "D:\projects\backup-genie\bin\Debug\net10.0-windows\BackupGenie.exe"
```

## Publish A Release Build

Create an optimized Release publish:

```powershell
dotnet publish -c Release
```

Or:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" publish -c Release
```

Published output is created in:

```text
D:\projects\backup-genie\bin\Release\net10.0-windows\publish\
```

Run the published app:

```powershell
& "D:\projects\backup-genie\bin\Release\net10.0-windows\publish\BackupGenie.exe"
```

## What To Share With Other Users

For a simple shared build, send the contents of the publish folder:

```text
bin\Release\net10.0-windows\publish\
```

At minimum, users should receive the full publish directory, not only `BackupGenie.exe`, because the app also depends on adjacent runtime and dependency files.

## Packaging Option 1: ZIP Archive

This is the fastest way to share the app internally.

1. Publish the Release build.
2. Open:

```text
D:\projects\backup-genie\bin\Release\net10.0-windows\publish\
```

3. Select all files in that folder.
4. Right-click and choose `Send to > Compressed (zipped) folder`, or use 7-Zip.
5. Name the archive something like:

```text
BackupGenie-win-x64-release.zip
```

6. Share the zip with users.

Recommended instructions for recipients:

- Extract the zip to a normal writable folder such as `C:\Apps\BackupGenie`
- Run `BackupGenie.exe`
- On first launch, choose a central backup folder and add backup jobs

## Packaging Option 2: Inno Setup Installer

Use this option if you want a friendlier install experience for other users.

### Install Inno Setup

Download and install [Inno Setup](https://jrsoftware.org/isinfo.php).

### Example Installer Script

Create a file named `BackupGenie.iss` next to the project or in a release folder with content like this:

```iss
[Setup]
AppName=Backup Genie
AppVersion=1.0.0
DefaultDirName={autopf}\Backup Genie
DefaultGroupName=Backup Genie
OutputDir=installer-output
OutputBaseFilename=BackupGenieSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "bin\Release\net10.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Backup Genie"; Filename: "{app}\BackupGenie.exe"
Name: "{autodesktop}\Backup Genie"; Filename: "{app}\BackupGenie.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\BackupGenie.exe"; Description: "Launch Backup Genie"; Flags: nowait postinstall skipifsilent
```

### Build The Installer

1. Publish the app in Release mode.
2. Open the `.iss` file in Inno Setup Compiler.
3. Click `Build`.
4. Share the generated installer from:

```text
installer-output\
```

## GitHub Actions Release Pipeline

This repository includes a manual GitHub Actions workflow at:

```text
.github/workflows/release.yml
```

It is triggered with `workflow_dispatch` and is designed to:

- build the app on `windows-latest`
- publish a self-contained single-file `win-x64` executable
- upload the published files as workflow artifacts
- create a GitHub Release
- attach both a `.zip` archive and the generated `BackupGenie.exe`

### How To Use The Workflow

1. Push the repository to GitHub.
2. Open the repository in GitHub.
3. Go to `Actions`.
4. Open the `Build And Release` workflow.
5. Click `Run workflow`.
6. Enter a tag such as:

```text
v1.0.0
```

7. Optionally enter a release title.
8. Choose whether the release should be marked as `draft` or `prerelease`.
9. Run the workflow.

### GitHub Release Output

After a successful run, GitHub will create a Release containing:

- `BackupGenie-<tag>-win-x64.zip`
- `BackupGenie.exe`

The workflow also stores the publish output as a workflow artifact for download from the Actions run page.

## Suggested Release Checklist

Before sharing a build with other users:

1. Run `dotnet build`
2. Run `dotnet publish -c Release`
3. Open the published app once and verify it starts
4. Add one file backup job and one folder backup job
5. Confirm a backup is created in the configured central folder
6. Confirm the app minimizes to tray and reopens correctly
7. Package the publish output as a zip or installer

## Screenshots To Capture

I could not capture live GUI screenshots from this environment, so this section gives you the exact screenshots to take on your machine and where to place them if you want them in project docs or release notes.

Recommended screenshot folder:

```text
D:\projects\backup-genie\docs\screenshots\
```

Recommended screenshots:

1. Main window after launch
   Show the dashboard with the backup destination, job list, and background behavior panel visible.
2. Add file or folder flow
   Show the app after adding at least one file and one folder backup job.
3. Background behavior settings
   Show the tray-related settings and auto-start option.
4. Successful backup result
   Show the `Last Result`, `Last Success`, and selected job summary after a completed run.
5. System tray menu
   Show the tray icon context menu with `Open`, `Run all backups now`, and `Exit`.

## Screenshot Markdown Example

If you save screenshots into `docs\screenshots`, you can reference them in Markdown like this:

```md
![Main window](docs/screenshots/main-window.png)
![Configured backup jobs](docs/screenshots/jobs-configured.png)
![Tray menu](docs/screenshots/tray-menu.png)
```

## Common Problems

### `dotnet` is not recognized

Use:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" build
```

Then restart PowerShell so the updated PATH is picked up.

### The app starts but no backups run

Check the following:

- a central backup location is selected
- at least one backup job is enabled
- the source path still exists
- the app is still running in the tray if the window was closed

### Backups are copying into themselves

Do not choose a central backup folder inside a source folder you are backing up. The app includes a guard against this, but it is still best to avoid that setup.

## Notes For Distributing To Others

- Settings are stored per user under `%LOCALAPPDATA%\BackupGenie\settings.json`
- Each backup job is stored separately in the app settings file
- The current implementation performs incremental mirror-style copies of current files
- The app does not yet provide version history, encryption, compression, or retention policies
