using Microsoft.AspNetCore.Mvc;

namespace be.SaveDownload;

[ApiController]
public class SaveDownloadController(SaveDownloadService service) : ControllerBase
{
    [HttpPost("api/downloads")]
    public async Task<IActionResult> DownloadAsync([FromBody] SaveDownloadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.DownloadAsync(request, cancellationToken);
            return result.IsSuccess
                ? StatusCode(result.StatusCode)
                : StatusCode(result.StatusCode, result.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
