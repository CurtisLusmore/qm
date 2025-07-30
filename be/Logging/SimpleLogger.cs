
using Microsoft.Extensions.Options;

namespace qm.Logging;

/// <summary>
/// A simple console logger
/// </summary>
/// <param name="config">The config</param>
public class SimpleLogger(IOptions<SimpleLoggerConfig> config) : ILogger
{
    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= config.Value.MinimumLoggingLevel;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        WritePrefix(logLevel);
        Console.WriteLine(formatter(state, exception));
        Console.ResetColor();
    }

    private static void WritePrefix(LogLevel logLevel)
    {
        Console.ResetColor();
        switch (logLevel)
        {
            case LogLevel.Trace:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("trc ");
                break;

            case LogLevel.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("dbg ");
                Console.ResetColor();
                break;

            case LogLevel.Information:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("inf ");
                Console.ResetColor();
                break;

            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("wrn ");
                Console.ResetColor();
                break;

            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("err ");
                Console.ResetColor();
                break;

            case LogLevel.Critical:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("crt ");
                break;
        }
    }
}
