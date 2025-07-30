using System.Reflection;
using qm;
using qm.Logging;
using qm.Services;

namespace Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Service Collection Extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add logging via the <see cref="SimpleLogger"/>
    /// </summary>
    /// <param name="builder">The Web Application Builder</param>
    /// <returns>The Web Application Builder</returns>
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        var config = new ManualOptions<SimpleLoggerConfig>
        {
            Value = new SimpleLoggerConfig
            {
                MinimumLoggingLevel = LogLevel.Information,
            },
        };
        builder.Services
            .AddSingleton(config)
            .AddSingleton<LogLevelService>()
            .AddHostedService(sp => sp.GetRequiredService<LogLevelService>());
        builder.Logging
            .SetMinimumLevel(LogLevel.Trace)
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .ClearProviders()
            .AddProvider(new SimpleLoggerProvider(config));
        return builder;
    }

    /// <summary>
    /// Add API Documentation via Swagger
    /// </summary>
    /// <param name="builder">The Web Application Builder</param>
    /// <returns>The Web Application Builder</returns>
    public static WebApplicationBuilder AddApiDocs(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services
                .AddEndpointsApiExplorer()
                .AddSwaggerGen(options =>
                {
                    options.IncludeXmlComments(
                        Path.Join(
                            AppContext.BaseDirectory,
                            $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
                });
        }
        return builder;
    }

    /// <summary>
    /// Add the Search service
    /// </summary>
    /// <param name="builder">The Web Application Builder</param>
    /// <returns>The Web Application Builder</returns>
    public static WebApplicationBuilder AddSearchService(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddHttpClient()
            .AddScoped<SearchService>();
        return builder;
    }

    /// <summary>
    /// Add the Torrent service
    /// </summary>
    /// <param name="builder">The Web Application Builder</param>
    /// <param name="directoryRoot">The root directory where files are saved</param>
    /// <returns>The Web Application Builder</returns>
    public static WebApplicationBuilder AddTorrentService(this WebApplicationBuilder builder, string? directoryRoot = null)
    {
        if (directoryRoot is not null)
        {
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{TorrentServiceConfig.SectionName}:{nameof(TorrentServiceConfig.DirectoryRoot)}"] = directoryRoot,
            });
        }

        builder.Services
            .Configure<TorrentServiceConfig>(builder.Configuration.GetSection(TorrentServiceConfig.SectionName))
            .AddSingleton<TorrentService>()
            .AddHostedService(sp => sp.GetRequiredService<TorrentService>());
        return builder;
    }

    /// <summary>
    /// Use API Documentation via Swagger
    /// </summary>
    /// <param name="app">The Web Application</param>
    /// <returns>The Web Application</returns>
    public static WebApplication UseApiDocs(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app
                .UseSwagger()
                .UseSwaggerUI();
        }
        return app;
    }
}
