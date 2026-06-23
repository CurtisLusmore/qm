using Microsoft.AspNetCore.Mvc;

namespace be.Search;

[ApiController]
[Route("")]
public class SearchController(SearchService service) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchAsync([FromQuery] string query)
    {
        try
        {
            var results = await service.SearchAsync(query);
            return results.IsSuccess
                ? StatusCode(results.StatusCode, results.Value)
                : StatusCode(results.StatusCode, results.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
