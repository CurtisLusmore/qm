using be.Interfaces;
using be.Models;
using System.Runtime.CompilerServices;

namespace be.Subscribe;

public class SubscribeService(IEventStream eventStream)
{
    public async IAsyncEnumerable<Event> Subscribe([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await foreach (var @event in eventStream.Subscribe(cancellationToken))
            {
                yield return @event;
            }
        }
    }
}
