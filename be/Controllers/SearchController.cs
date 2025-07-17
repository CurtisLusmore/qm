using Microsoft.AspNetCore.Mvc;
using qm.Dtos;

namespace qm.Controllers;

/// <summary>
/// Search controller
/// </summary>
[ApiController]
[Route("api/search")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly TorrentSearchResult[] searchResults = new TorrentSearchResult[]
    {
        new TorrentSearchResult(
            "0123456789abcdef", "Torrent name",
            10, 2_000, 2),
    };

    /// <summary>
    /// Search for torrents
    /// </summary>
    /// <param name="terms">The search terms to use</param>
    /// <returns>Search results</returns>
    /// <response code="200">Search results</response>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TorrentSearchResult))]
    public Task<IActionResult> SearchTorrentsAsync([FromQuery] string? terms)
    {
        return Task.FromResult<IActionResult>(Ok(searchResults));
    }
}