using System.Web;
using qm.Dtos;

namespace qm.Services;

/// <summary>
/// Service for searching apibay.org for torrents
/// </summary>
/// <param name="client">An HTTP client</param>
public class SearchService(HttpClient client)
{
    private const string baseUrl = "https://apibay.org/";

    /// <summary>
    /// Search apibay.org for torrents
    /// </summary>
    /// <param name="terms">The search terms to use</param>
    /// <returns>The search results</returns>
    public async Task<IEnumerable<TorrentSearchResult>> SearchAsync(string? terms)
    {
        var results = (await client.GetFromJsonAsync<ApiBaySearchResult[]>($"{baseUrl}q.php?q={HttpUtility.UrlEncode(terms)}"))
            ?? [];
        return results.Length == 1 && results.Single().id == "0"
            ? []
            : results.Select(result => (TorrentSearchResult)result).ToArray();
    }

    private record ApiBaySearchResult(
        string id,
        string name,
        string info_hash,
        string seeders,
        string num_files,
        string size)
    {
        public static explicit operator TorrentSearchResult(ApiBaySearchResult result)
            => new TorrentSearchResult(
                result.info_hash,
                result.name,
                int.Parse(result.seeders),
                long.Parse(result.size),
                int.Parse(result.num_files));
    }
}
