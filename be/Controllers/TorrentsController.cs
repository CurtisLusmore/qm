using Microsoft.AspNetCore.Mvc;
using qm.Dtos;

namespace qm.Controllers;

/// <summary>
/// Torrents controller
/// </summary>
[ApiController]
[Route("api/torrents")]
[Produces("application/json")]
public class TorrentsController : ControllerBase
{
    private readonly Torrent[] torrents = new Torrent[]
    {
        new Torrent(
            "0123456789abcdef", "Torrent name", State.Downloading,
            10, 1_000, 2_000, 50, 2,
            [
                new TorrentFile("File1.txt", 750, 1000, 75, Priority.High),
                new TorrentFile("File2.txt", 250, 1000, 25, Priority.Normal),
            ]),
    };

    /// <summary>
    /// Get all saved torrents
    /// </summary>
    /// <returns>The saved torrents</returns>
    /// <response code="200">The saved torrents</response>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Torrent>))]
    public Task<IActionResult> GetTorrentsAsync()
    {
        return Task.FromResult<IActionResult>(Ok(torrents));
    }

    /// <summary>
    /// Get a saved torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to get</param>
    /// <returns>The saved torrent</returns>
    /// <response code="200">The saved torrent</response>
    /// <response code="404">Torrent not found</response>
    [HttpGet("{infoHash}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Torrent>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    public Task<IActionResult> GetTorrentAsync(string infoHash)
    {
        var torrent = torrents.SingleOrDefault(torrent => torrent.InfoHash == infoHash);
        return Task.FromResult<IActionResult>(
            torrent is not null
                ? Ok(torrent)
                : NotFound());
    }

    /// <summary>
    /// Save a torrent to start downloading
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to save</param>
    /// <response code="202">The request has been accepted</response>
    [HttpPost("{infoHash}")]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = null!)]
    public Task<IActionResult> SaveTorrentAsync(string infoHash)
    {
        return Task.FromResult<IActionResult>(Accepted());
    }

    /// <summary>
    /// Remove a torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to remove</param>
    /// <response code="202">The request has been accepted</response>
    [HttpDelete("{infoHash}")]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = null!)]
    public Task<IActionResult> RemoveTorrentAsync(string infoHash)
    {
        var torrent = torrents.SingleOrDefault(torrent => torrent.InfoHash == infoHash);
        return Task.FromResult<IActionResult>(
            torrent is not null
                ? NoContent()
                : NotFound());
    }


    /// <summary>
    /// Update the state or priority of a torrent or its files
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to update</param>
    /// <param name="patch">The updates to make</param>
    /// <response code="202">The request has been accepted</response>
    [HttpPatch("{infoHash}")]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = null!)]
    public Task<IActionResult> UpdateTorrentAsync(string infoHash, TorrentPatch patch)
    {
        var torrent = torrents.SingleOrDefault(torrent => torrent.InfoHash == infoHash);
        return Task.FromResult<IActionResult>(
            torrent is not null
                ? NoContent()
                : NotFound());
    }
}