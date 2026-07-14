using Microsoft.AspNetCore.Mvc;

namespace be.Search;

[ApiController]
public class SearchController(SearchService service) : ControllerBase
{
    [HttpGet("api/search")]
    public async Task<IActionResult> SearchAsync([FromQuery] string query, CancellationToken cancellationToken)
    {
        try
        {
            var results = await service.SearchAsync(query, cancellationToken);
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
