using Microsoft.AspNetCore.Mvc;

namespace be.RemoveTitle;

[ApiController]
public class RemoveTitleController(RemoveTitleService service) : ControllerBase
{
    [HttpDelete("movies/{titleId}")]
    public async Task<IActionResult> RemoveMovieAsync([FromRoute] string titleId)
    {
        try
        {
            var result = await service.RemoveMovieAsync(titleId);
            return result.IsSuccess
                ? StatusCode(result.StatusCode, result.Value)
                : StatusCode(result.StatusCode, result.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("series/{titleId}")]
    public async Task<IActionResult> RemoveSeriesAsync([FromRoute] string titleId)
    {
        try
        {
            var result = await service.RemoveSeriesAsync(titleId);
            return result.IsSuccess
                ? StatusCode(result.StatusCode, result.Value)
                : StatusCode(result.StatusCode, result.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
