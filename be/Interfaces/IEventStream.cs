using be.Models;

namespace be.Interfaces;

public interface IEventStream
{
    IAsyncEnumerable<Event> Subscribe(CancellationToken cancellationToken);
    Task SendEventAsync(Event @event);
}
