using Microsoft.AspNetCore.Mvc;

namespace be.GetCollection;

[ApiController]
public class GetCollectionController(GetCollectionService service): ControllerBase
{
    [HttpGet("collection")]
    public async Task<IActionResult> GetCollectionAsync()
    {
        try
        {
            var result = await service.GetCollectionAsync();
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
