using System.Reflection;
using Microsoft.Extensions.Logging.Console;
using qm;
using qm.Services;

namespace Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Service Collection Extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        builder.Logging
            .SetMinimumLevel(LogLevel.Debug)
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddConsole(opts => opts.FormatterName = nameof(MinimalConsoleFormatter));
        builder.Services.AddSingleton<ConsoleFormatter, MinimalConsoleFormatter>();
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
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
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<SearchService>();
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

        builder.Services.Configure<TorrentServiceConfig>(builder.Configuration.GetSection(TorrentServiceConfig.SectionName));
        builder.Services.AddSingleton<TorrentService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<TorrentService>());
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
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }
}
