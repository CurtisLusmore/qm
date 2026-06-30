using Microsoft.AspNetCore.Mvc;

namespace be.Subscribe;

[ApiController]
public class SubscribeController(SubscribeService service) : ControllerBase
{
    [HttpGet("api/subscribe")]
    public IResult Subscribe(CancellationToken cancellationToken)
    {
        return TypedResults.ServerSentEvents(service.Subscribe(cancellationToken));
    }
}
