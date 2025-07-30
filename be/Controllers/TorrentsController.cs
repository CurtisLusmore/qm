using Microsoft.AspNetCore.Mvc;
using qm.Dtos;
using qm.Services;

namespace qm.Controllers;

/// <summary>
/// Torrents controller
/// </summary>
[ApiController]
[Route("api/torrents")]
[Produces("application/json")]
public class TorrentsController(TorrentService service) : ControllerBase
{
    /// <summary>
    /// Get all saved torrents
    /// </summary>
    /// <returns>The saved torrents</returns>
    /// <response code="200">The saved torrents</response>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Torrent>))]
    public IActionResult GetTorrentsAsync()
    {
        var torrents = service.GetTorrents();
        return Ok(torrents);
    }

    /// <summary>
    /// Get a saved torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to get</param>
    /// <returns>The saved torrent</returns>
    /// <response code="200">The saved torrent</response>
    /// <response code="404">Torrent not found</response>
    [HttpGet("{infoHash}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Torrent))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    public IActionResult GetTorrentAsync(string infoHash)
    {
        var torrent = service.GetTorrent(infoHash);
        return torrent is not null
            ? Ok(torrent)
            : NotFound();
    }

    /// <summary>
    /// Save a torrent to start downloading
    /// </summary>
    /// <param name="torrent">The details of the torrent to save</param>
    /// <response code="202">The request has been accepted</response>
    /// <response code="400">Torrent could not be saved</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = null!)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = null!)]
    public IActionResult SaveTorrentAsync(SaveTorrent torrent)
    {
        var accepted = service.SaveTorrent(torrent);
        return accepted
            ? Accepted()
            : BadRequest();
    }

    /// <summary>
    /// Remove a torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to remove</param>
    /// <response code="202">The request has been accepted</response>
    /// <response code="404">Torrent not found</response>
    [HttpDelete("{infoHash}")]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    public IActionResult RemoveTorrentAsync(string infoHash)
    {
        var accepted = service.RemoveTorrent(infoHash);
        return accepted
            ? Accepted()
            : NotFound();
    }


    /// <summary>
    /// Update the state or priority of a torrent or its files
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to update</param>
    /// <param name="patch">The updates to make</param>
    /// <response code="202">The request has been accepted</response>
    /// <response code="404">Torrent not found</response>
    [HttpPatch("{infoHash}")]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = null!)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = null!)]
    public IActionResult UpdateTorrentAsync(string infoHash, TorrentPatch patch)
    {
        var accepted = service.UpdateTorrent(infoHash, patch);
        return accepted
            ? Accepted()
            : NotFound();
    }
}