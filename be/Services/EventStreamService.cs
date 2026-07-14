using be.Interfaces;
using be.Models;

namespace be.Services;

public partial class DownloadManagementService : IEventStream
{
    public IAsyncEnumerable<Event> Subscribe(CancellationToken cancellationToken)
    {
        return eventStream.Subscribe(cancellationToken);
    }

    public Task SendEventAsync(Event @event)
    {
        return eventStream.SendEventAsync(@event);
    }
}
