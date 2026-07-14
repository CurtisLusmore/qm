using be.Models;
using Microsoft.AspNetCore.Mvc;

namespace be.PatchDownload;

public class PatchDownloadController(PatchDownloadService service) : ControllerBase
{
    [HttpPatch("api/downloads/{infoHash}")]
    public async Task<IActionResult> PatchDownloadAsync([FromRoute] string infoHash, [FromBody] DownloadPatch patch)
    {
        try
        {
            var result = await service.PatchDownloadAsync(infoHash, patch);
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
