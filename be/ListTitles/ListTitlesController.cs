using be.Models;
using Microsoft.AspNetCore.Mvc;

namespace be.ListTitles;

[ApiController]
public class ListTitlesController(ListTitlesService service) : ControllerBase
{
    [HttpGet("api/movies")]
    public IAsyncEnumerable<Movie> ListMoviesAsync([FromQuery] DateTime since, CancellationToken cancellationToken)
    {
        return service.ListMoviesAsync(since, cancellationToken);
    }

    [HttpGet("api/series")]
    public IAsyncEnumerable<Series> ListSeriesAsync([FromQuery] DateTime since, CancellationToken cancellationToken)
    {
        return service.ListSeriesAsync(since, cancellationToken);
    }
}
