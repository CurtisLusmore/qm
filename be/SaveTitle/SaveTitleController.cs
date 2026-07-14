using be.Models;
using Microsoft.AspNetCore.Mvc;

namespace be.SaveTitle;

[ApiController]
public class SaveTitleController(SaveTitleService service) : ControllerBase
{
    [HttpPost("api/movies")]
    public async Task<IActionResult> SaveMovieAsync([FromBody] Movie title, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.SaveMovieAsync(title, cancellationToken);
            return result.IsSuccess
                ? StatusCode(result.StatusCode, result.Value)
                : StatusCode(result.StatusCode, result.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("api/series")]
    public async Task<IActionResult> SaveSeriesAsync([FromBody] Series title, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.SaveSeriesAsync(title, cancellationToken);
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
