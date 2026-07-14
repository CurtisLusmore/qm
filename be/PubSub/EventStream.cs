using System.Threading.Channels;
using be.Models;

namespace be.PubSub;

public class EventStream
{
    private readonly Channel<Event> _channels = Channel.CreateUnbounded<Event>();
    public IAsyncEnumerable<Event> Subscribe(CancellationToken cancellationToken)
        => _channels.Reader.ReadAllAsync(cancellationToken);

    public async Task SendEventAsync(Event @event)
    {
        await _channels.Writer.WriteAsync(@event);
    }
}
