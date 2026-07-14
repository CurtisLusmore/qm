using Microsoft.AspNetCore.Mvc;

namespace be.RemoveDownload;

public class RemoveDownloadController(RemoveDownloadService service) : ControllerBase
{
    [HttpDelete("api/downloads/{infoHash}")]
    public async Task<IActionResult> RemoveDownloadAsync([FromRoute] string infoHash)
    {
        try
        {
            var result = await service.RemoveDownloadAsync(infoHash);
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
