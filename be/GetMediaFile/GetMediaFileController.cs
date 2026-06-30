using Microsoft.AspNetCore.Mvc;

namespace be.GetMediaFile;

[ApiController]
public class GetMediaFileController(GetMediaFileService service): ControllerBase
{
    [HttpHead("api/movies/{titleId}/media")]
    [HttpGet("api/movies/{titleId}/media")]
    public IActionResult GetMovieMediaFile([FromRoute] string titleId)
    {
        try
        {
            var result = service.GetMovieMediaFile(titleId);
            return result.IsSuccess
                ? PhysicalFile(result.Value.FilePath, result.Value.MediaType, enableRangeProcessing: true)
                : StatusCode(result.StatusCode, result.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpHead("api/series/{titleId}/{seasonNumber}/{episodeNumber}/media")]
    [HttpGet("api/series/{titleId}/{seasonNumber}/{episodeNumber}/media")]
    public IActionResult GetEpisodeMediaFile([FromRoute] string titleId, int seasonNumber, int episodeNumber)
    {
        try
        {
            var result = service.GetEpisodeMediaFile(titleId, seasonNumber, episodeNumber);
            return result.IsSuccess
                ? PhysicalFile(result.Value.FilePath, result.Value.MediaType, enableRangeProcessing: true)
                : StatusCode(result.StatusCode, result.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
