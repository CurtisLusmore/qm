using Microsoft.AspNetCore.Mvc;

namespace be.ListDownloads;

[ApiController]
public class ListDownloadsController(ListDownloadsService service) : ControllerBase
{
    [HttpGet("api/downloads")]
    public async Task<IActionResult> ListDownloadsAsync()
    {
        try
        {
            var result = await service.ListDownloadsAsync();
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
