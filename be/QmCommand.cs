using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;

namespace qm;

/// <summary>
/// Root command
/// </summary>
public class QmCommand : RootCommand
{
    /// <summary>
    /// Root command
    /// </summary>
    /// <param name="builder">The Web Application Builder</param>
    public QmCommand(WebApplicationBuilder builder)
        : base("Quartermaster")
    {
        var rootOpt = new Option<string>("--root") { Description = "Download directory root", DefaultValueFactory = _ => Environment.CurrentDirectory };
        var portOpt = new Option<int>("--port") { Description = "Listening port", DefaultValueFactory = _ => 8080 };

        Add(rootOpt);
        Add(portOpt);
        SetAction(parseResult =>
        {
            var root = parseResult.GetRequiredValue(rootOpt);
            var port = parseResult.GetRequiredValue(portOpt);

            if (string.IsNullOrWhiteSpace(root)) root = Environment.CurrentDirectory;
            root = Path.IsPathFullyQualified(root)
                ? root
                : Path.Join(Environment.CurrentDirectory, root);

            builder.WebHost.UseKestrel(options => options.ListenAnyIP(port));

            builder.AddLogging();
            builder.AddApiDocs();
            builder.AddSearchService();
            builder.AddTorrentService(root);
            builder.Services.AddControllers();
            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("cors", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .WithMethods("GET", "POST", "PATCH", "DELETE")
                        .WithHeaders("Content-Type");
                });
            });

            var app = builder.Build();

            app.UseApiDocs();

            app.MapControllers();
            app.UseCors("cors");
            app.UseFileServer(
                new FileServerOptions
                {
                    FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetAssembly(typeof(Program))!, "wwwroot"),
                });

            var logger = app.Services.GetRequiredService<ILogger<QmCommand>>();
            var ip = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => $"http://{ip}:{port}/")
                .First();
            logger.LogInformation("Application listening at {address}", ip);

            Process.Start(new ProcessStartInfo($"http://localhost:{port}") { UseShellExecute = true });

            app.Run();
        });
    }
}
