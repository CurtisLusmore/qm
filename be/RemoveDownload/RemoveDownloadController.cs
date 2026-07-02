using Microsoft.AspNetCore.Mvc;

namespace be.RemoveDownload;

public class RemoveDownloadController(RemoveDownloadService service) : ControllerBase
{
    [HttpDelete("api/downloads/{infoHash}")]
    public IActionResult RemoveDownload([FromRoute] string infoHash)
    {
        try
        {
            var result = service.RemoveDownload(infoHash);
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
