using System.Collections;
using be.Interfaces;
using be.Shared;

namespace be.HostedServices;

public partial class DownloadManagementService : IDownloadLister
{
    public IEnumerator<DownloadTracker> GetEnumerator()
        => trackers.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
