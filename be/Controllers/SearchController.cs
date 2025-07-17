using Microsoft.AspNetCore.Mvc;
using qm.Dtos;
using qm.Services;

namespace qm.Controllers;

/// <summary>
/// Search controller
/// </summary>
[ApiController]
[Route("api/search")]
[Produces("application/json")]
public class SearchController(SearchService service) : ControllerBase
{
    /// <summary>
    /// Search for torrents
    /// </summary>
    /// <param name="terms">The search terms to use</param>
    /// <returns>Search results</returns>
    /// <response code="200">Search results</response>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TorrentSearchResult))]
    public async Task<IActionResult> SearchTorrentsAsync([FromQuery] string? terms)
    {
        var results = await service.SearchAsync(terms);
        return Ok(results);
    }
}