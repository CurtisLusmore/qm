using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace qm.Logging;

/// <summary>
/// A provider for the <see cref="SimpleLogger"/>
/// </summary>
/// <param name="config">The config</param>
public class SimpleLoggerProvider(IOptions<SimpleLoggerConfig> config) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ILogger> loggers = new();
    private readonly IOptions<SimpleLoggerConfig> config = config;

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => loggers.GetOrAdd(categoryName, name => new SimpleLogger(config));

    /// <inheritdoc/>
    public void Dispose() => loggers.Clear();
}
