using be.Shared;
using Microsoft.AspNetCore.Mvc;

namespace be.SaveTitle;

[ApiController]
public class SaveTitleController(SaveTitleService service) : ControllerBase
{
    [HttpPost("movies")]
    public async Task<IActionResult> SaveMovieAsync([FromBody] Movie title)
    {
        try
        {
            var result = await service.SaveMovieAsync(title);
            return result.IsSuccess
                ? StatusCode(result.StatusCode, result.Value)
                : StatusCode(result.StatusCode, result.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("series")]
    public async Task<IActionResult> SaveSeriesAsync([FromBody] Series title)
    {
        try
        {
            var result = await service.SaveSeriesAsync(title);
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
