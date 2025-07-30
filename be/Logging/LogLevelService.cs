
namespace qm.Logging;

/// <summary>
/// Service for setting the minimum log level
/// </summary>
/// <param name="options">The options object to update</param>
/// <param name="logger">A logger</param>
public class LogLevelService(
        ManualOptions<SimpleLoggerConfig> options,
        ILogger<LogLevelService> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!Console.KeyAvailable)
            {
                await Task.Delay(100, stoppingToken);
                continue;
            }

            var key = Console.ReadKey(true);
            LogLevel logLevel;
            switch (key.Key)
            {
                case ConsoleKey.T:
                    logLevel = LogLevel.Trace;
                    break;
                case ConsoleKey.D:
                    logLevel = LogLevel.Debug;
                    break;
                case ConsoleKey.I:
                    logLevel = LogLevel.Information;
                    break;
                case ConsoleKey.W:
                    logLevel = LogLevel.Warning;
                    break;
                case ConsoleKey.E:
                    logLevel = LogLevel.Error;
                    break;
                case ConsoleKey.C:
                    logLevel = LogLevel.Critical;
                    break;
                default:
                    continue;
            }
            options.SetValue(
                new SimpleLoggerConfig
                {
                    MinimumLoggingLevel = logLevel,
                });
            logger.Log(logLevel, "Changed minimum log level to {logLevel}", logLevel);
        }
    }
}
