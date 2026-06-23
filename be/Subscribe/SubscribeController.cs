using Microsoft.AspNetCore.Mvc;

namespace be.Subscribe;

[ApiController]
public class SubscribeController(SubscribeService service) : ControllerBase
{
    [HttpGet("subscribe")]
    public IResult Subscribe(CancellationToken cancellationToken)
    {
        return TypedResults.ServerSentEvents(service.Subscribe(cancellationToken));
    }
}
