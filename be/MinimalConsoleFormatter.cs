using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace qm;

/// <summary>
/// Minimal Console Formatter
/// </summary>
public class MinimalConsoleFormatter() : ConsoleFormatter(nameof(MinimalConsoleFormatter))
{
    /// <inheritdoc/>
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        LogLevelPrefix(logEntry.LogLevel);
        textWriter.WriteLine(logEntry.State?.ToString());
    }

    private void LogLevelPrefix(LogLevel level)
    {
        var originalColor = Console.ForegroundColor;
        try
        {
            switch (level)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Out.Write("trce: ");
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Out.Write("dbug: ");
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Out.Write("info: ");
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Out.Write("warn: ");
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Out.Write("err:  ");
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Out.Write("crit: ");
                    break;
            }
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }
}
