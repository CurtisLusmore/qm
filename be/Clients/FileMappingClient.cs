using be.Models;

namespace be.Clients;

public class FileMappingClient
{
    public IEnumerable<FileMapping> MapFiles(MovieOrSeries title, IEnumerable<MonoTorrent.ITorrentManagerFile> files)
    {
        var mappings = new List<FileMapping>();

        switch (title.Type)
        {
            case TitleType.Movie:
                var destinationFilename = $"{title.Name} ({title.Year})";
                foreach (var file in files)
                {
                    if (IsMediaFile(file.Path))
                    {
                        var extension = Path.GetExtension(file.Path);
                        var priority = IsVideoFile(file.Path) ? FilePriority.High : FilePriority.Normal;
                        var mapping = new FileMapping(
                            file,
                            $"{destinationFilename}{extension}",
                            priority);
                        mappings.Add(mapping);
                    }
                    else
                    {
                        mappings.Add(new FileMapping(file, file.Path, FilePriority.Skip));
                    }
                }
                break;

            case TitleType.Series:
                foreach (var file in files)
                {
                    if (IsMediaFile(file.Path))
                    {
                        var extension = Path.GetExtension(file.Path);
                        var episodeInfo = ExtractEpisodeInfo(file.Path);
                        var episode = GetEpisode(episodeInfo);
                        var priority = IsVideoFile(file.Path) ? FilePriority.High : FilePriority.Normal;
                        var mapping = episode is null
                            ? new FileMapping(file, file.Path, priority)
                            : new FileMapping(
                                file,
                                $"S{episodeInfo!.SeasonNumber:D2}E{episodeInfo.EpisodeNumber:D2} {episode?.Name}{extension}",
                                priority,
                                episodeInfo);
                        mappings.Add(mapping);
                    }
                    else
                    {
                        mappings.Add(new FileMapping(file, file.Path, FilePriority.Skip));
                    }
                }
                break;

            default:
                throw new ArgumentException($"Invalid title type for mapping: {title.Type}");
        }

        return mappings;

        EpisodeInfo? ExtractEpisodeInfo(string filePath)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"[Ss](\d{2})[Ee](\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var match = regex.Match(filePath);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int season) && int.TryParse(match.Groups[2].Value, out int episode))
            {
                return new(season, episode);
            }
            return null;
        }

        Episode? GetEpisode(EpisodeInfo? episodeInfo)
        {
            return episodeInfo is null
                ? null
                : title
                    .Episodes?
                    .SingleOrDefault(ep => ep.SeasonNumber == episodeInfo.SeasonNumber && ep.EpisodeNumber == episodeInfo.EpisodeNumber);
        }
    }

    private static bool IsVideoFile(string filePath)
    {
        var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm"
        };

        return videoExtensions.Contains(Path.GetExtension(filePath));
    }

    private static bool IsMediaFile(string filePath)
    {
        var mediaExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".srt"
        };

        return mediaExtensions.Contains(Path.GetExtension(filePath));
    }
}
