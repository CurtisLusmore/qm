using System.Net;
using be.Models;

namespace be.Search;

public class SearchService(
    IHttpClientFactory httpClientFactory,
    ILogger<SearchService> logger)
{
    const string BaseUrl = "https://apibay.org/q.php?q=";

    public async Task<Result<IEnumerable<SearchResult>>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; SearchService/1.0)");
        Response[]? response;
        try
        {
            response = await client.GetFromJsonAsync<Response[]>($"{BaseUrl}{Uri.EscapeDataString(query)}", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error searching for query: {Query}", query);
            return Result<IEnumerable<SearchResult>>.Failure(ex.Message, ex.StatusCode ?? HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while searching for query: {Query}", query);
            throw;
        }

        var results = response?
            .Where(r => r.id != "0")
            .Select(Map) ?? [];

        return Result<IEnumerable<SearchResult>>.Success(results);
    }

    private SearchResult Map(Response response)
        => new(
            response.info_hash,
            response.name,
            long.Parse(response.size),
            int.Parse(response.seeders));

    private record Response(
        string id,
        string name,
        string info_hash,
        string leechers,
        string seeders,
        string size,
        string num_files,
        string username,
        string added,
        string status,
        string category,
        string imdb);
}
