using System.IO;
using BackupGenie.Models;

namespace BackupGenie.Services;

public sealed class BackupRunner
{
    public async Task<BackupRunResult> RunAsync(BackupJob job, string centralBackupLocation)
    {
        if (string.IsNullOrWhiteSpace(job.SourcePath))
        {
            throw new InvalidOperationException("The source path is empty.");
        }

        if (!File.Exists(job.SourcePath) && !Directory.Exists(job.SourcePath))
        {
            throw new DirectoryNotFoundException($"The source path was not found: {job.SourcePath}");
        }

        if (Directory.Exists(job.SourcePath) && IsNestedPath(centralBackupLocation, job.SourcePath))
        {
            throw new InvalidOperationException(
                "The central backup folder cannot live inside a folder that is also being backed up.");
        }

        Directory.CreateDirectory(centralBackupLocation);
        var jobRoot = Path.Combine(centralBackupLocation, $"{Sanitize(job.DisplayName)}_{job.Id:N}");
        Directory.CreateDirectory(jobRoot);

        return await Task.Run(() =>
        {
            var result = new BackupRunResult();
            if (File.Exists(job.SourcePath))
            {
                BackupSingleFile(job.SourcePath, jobRoot, result);
            }
            else
            {
                BackupDirectory(job.SourcePath, jobRoot, result);
            }

            return result;
        });
    }

    private static void BackupSingleFile(string sourcePath, string jobRoot, BackupRunResult result)
    {
        var destinationPath = Path.Combine(jobRoot, Path.GetFileName(sourcePath));
        result.FilesScanned++;
        if (CopyIfChanged(sourcePath, destinationPath))
        {
            result.FilesCopied++;
        }
    }

    private static void BackupDirectory(string sourceDirectory, string jobRoot, BackupRunResult result)
    {
        foreach (var sourceFile in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, sourceFile);
            var destinationPath = Path.Combine(jobRoot, relativePath);

            result.FilesScanned++;
            if (CopyIfChanged(sourceFile, destinationPath))
            {
                result.FilesCopied++;
            }
        }
    }

    private static bool CopyIfChanged(string sourceFile, string destinationFile)
    {
        var sourceInfo = new FileInfo(sourceFile);
        var destinationInfo = new FileInfo(destinationFile);

        if (destinationInfo.Exists &&
            destinationInfo.Length == sourceInfo.Length &&
            destinationInfo.LastWriteTimeUtc == sourceInfo.LastWriteTimeUtc)
        {
            return false;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
        File.Copy(sourceFile, destinationFile, true);
        File.SetLastWriteTimeUtc(destinationFile, sourceInfo.LastWriteTimeUtc);
        return true;
    }

    private static bool IsNestedPath(string candidateChildPath, string candidateParentPath)
    {
        var child = Path.GetFullPath(candidateChildPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parent = Path.GetFullPath(candidateParentPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return child.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || string.Equals(child, parent, StringComparison.OrdinalIgnoreCase);
    }

    private static string Sanitize(string value)
    {
        var fallback = string.IsNullOrWhiteSpace(value) ? "BackupJob" : value.Trim();
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            fallback = fallback.Replace(invalidCharacter, '_');
        }

        return fallback;
    }
}

public sealed class BackupRunResult
{
    public int FilesScanned { get; set; }

    public int FilesCopied { get; set; }
}
