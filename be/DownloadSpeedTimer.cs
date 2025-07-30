namespace qm;

/// <summary>
/// Measure download speed
/// </summary>
public class DownloadSpeedTimer
{
    private const int NumPulses = 10;
    private readonly List<(DateTime, long)> timestamps = [];

    /// <summary>
    /// The current download speed
    /// </summary>
    public double BytesPerSecond { get; private set; } = 0.0;

    /// <summary>
    /// Record a pulse and get the current download speed
    /// </summary>
    /// <param name="downloadedBytes">The current downloaded bytes</param>
    /// <returns>The current download speed</returns>
    public double Pulse(long downloadedBytes)
    {
        if (timestamps.Count == NumPulses) timestamps.RemoveAt(0);
        timestamps.Add((DateTime.UtcNow, downloadedBytes));
        if (timestamps.Count < 2) return 0;
        var (timeA, bytesA) = timestamps.First();
        var (timeB, bytesB) = timestamps.Last();
        return BytesPerSecond = (bytesB - bytesA) / (timeB - timeA).TotalSeconds;
    }

    /// <summary>
    /// Reset the timer
    /// </summary>
    public void Reset()
    {
        timestamps.Clear();
        BytesPerSecond = 0.0;
    }
}
