using be.Interfaces;
using System.Runtime.CompilerServices;

namespace be.Subscribe;

public class SubscribeService(IDownloadLister downloadLister)
{
    public async IAsyncEnumerable<Event> Subscribe([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return new Event([.. downloadLister]);
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}
