using be.Shared;
using System.Text.RegularExpressions;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task SortFilesAsync(CancellationToken cancellationToken)
    {
        foreach (var (_, tracker) in trackers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await SortFilesForTitleAsync(tracker, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error when sorting files for torrent {InfoHash}: {Reason}", tracker.InfoHash, ex.Message);
            }
        }
    }

    private async Task SortFilesForTitleAsync(DownloadTracker tracker, CancellationToken cancellationToken)
    {
        if (tracker.Status != DownloadStatus.DownloadedTorrent) return;

        switch (tracker.Title.Type)
        {
            case TitleType.Movie:
                await SortFilesForMovieAsync(tracker, cancellationToken);
                break;

            case TitleType.Series:
                await SortFilesForSeriesAsync(tracker, cancellationToken);
                break;

            default:
                logger.LogWarning("Unknown title type {TitleType} for torrent {InfoHash}", tracker.Title.Type, tracker.InfoHash);
                break;
        }

        await BinUnsortedFilesAsync(tracker, cancellationToken);

        trackers[tracker.InfoHash] = tracker with { Status = DownloadStatus.Completed };
    }

    private async Task SortFilesForMovieAsync(DownloadTracker tracker, CancellationToken cancellationToken)
    {
        var targetDirectory = moviesDirectory;
        var targetFilename = $"{tracker.Title.Name} ({tracker.Title.Year}) [{tracker.Title.Id}]";
        var sourceDirectory = TitleDirectory(completedDirectory, tracker.Title, tracker.InfoHash);

        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var extension = Path.GetExtension(file);
            if (IsJunkFile(extension)) continue;

            var targetFile = Path.Combine(targetDirectory, $"{targetFilename}{extension}");
            File.Move(file, targetFile, overwrite: true);
            logger.LogInformation("Moved {SourceFile} to {TargetFile}...", file, targetFile);
        }
    }

    private async Task SortFilesForSeriesAsync(DownloadTracker tracker, CancellationToken cancellationToken)
    {
        var targetDirectory = TitleDirectory(seriesDirectory, tracker.Title);
        Directory.CreateDirectory(targetDirectory);
        var sourceDirectory = TitleDirectory(completedDirectory, tracker.Title, tracker.InfoHash);

        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var extension = Path.GetExtension(file);
            if (IsJunkFile(extension))
            {
                File.Delete(file);
                logger.LogInformation("Deleted junk file {File}...", file);
                continue;
            }

            if (TryExtractSeasonEpisodeNumbers(Path.GetFileNameWithoutExtension(file), out var s, out var e))
            {
                var episode = tracker.Title.Episodes?.FirstOrDefault(ep => ep.SeasonNumber == s && ep.EpisodeNumber == e);
                if (episode != null)
                {
                    var targetFile = Path.Combine(targetDirectory, $"S{s:D2}E{e:D2} {FilenameSanitize(episode.Name)}{extension}");
                    File.Move(file, targetFile, overwrite: true);
                    logger.LogInformation("Moved {SourceFile} to {TargetFile}...", file, targetFile);
                }
                else
                {
                    var targetFile = Path.Combine(targetDirectory, $"S{s:D2}E{e:D2}{extension}");
                    File.Move(file, targetFile, overwrite: true);
                    logger.LogWarning("Moved {SourceFile} to {TargetFile}...", file, targetFile);
                }
            }
            else
            {
                var targetFile = Path.Combine(targetDirectory, Path.GetFileName(file));
                File.Move(file, targetFile, overwrite: true);
                logger.LogWarning("Could not extract season and episode numbers from {File}. Moved as is...", file);
            }
        }
    }

    private async Task BinUnsortedFilesAsync(DownloadTracker tracker, CancellationToken cancellationToken)
    {
        var sourceDirectory = TitleDirectory(completedDirectory, tracker.Title, tracker.InfoHash);
        var titleBinDirectory = TitleDirectory(binDirectory, tracker.Title, tracker.InfoHash);

        if (Directory.EnumerateFiles(sourceDirectory).Any())
        {
            Directory.Move(sourceDirectory, titleBinDirectory);
            logger.LogWarning("Moved {SourceFile} to {TargetFile}...", sourceDirectory, titleBinDirectory);
        }
        else
        {
            Directory.Delete(sourceDirectory);
            logger.LogInformation("Deleted empty source directory {SourceDirectory}...", sourceDirectory);
        }
    }

    private static string[] extensionsToKeep = [ ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".srt" ];

    private static bool IsJunkFile(string extension) => !extensionsToKeep.Contains(extension, StringComparer.OrdinalIgnoreCase);

    private static bool TryExtractSeasonEpisodeNumbers(string filename, out int season, out int episode)
    {
        season = 0;
        episode = 0;

        var match = Regex.Match(filename, @"[Ss](\d+)[Ee](\d+)");
        if (match.Success && match.Groups.Count == 3)
        {
            season = int.Parse(match.Groups[1].Value);
            episode = int.Parse(match.Groups[2].Value);
            return true;
        }

        return false;
    }

    private string FilenameSanitize(string filename)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(filename.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return sanitized;
    }
}
