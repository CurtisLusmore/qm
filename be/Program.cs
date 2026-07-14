using be.Clients;
using be.Config;
using be.GetMediaFile;
using be.Interfaces;
using be.ListTitles;
using be.PatchDownload;
using be.RemoveDownload;
using be.RemoveTitle;
using be.SaveDownload;
using be.SaveTitle;
using be.Search;
using be.Services;
using be.Subscribe;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrEmpty(builder.Configuration["Urls"]))
{
    builder.WebHost.UseUrls("http://0.0.0.0:1713");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddLogging();
builder.Services.AddHttpClient();
builder.Services.Configure<LibraryConfig>(builder.Configuration.GetSection("Library"));
builder.Services.AddSingleton<DownloadFileClient>();
builder.Services.AddSingleton<FastResumeFileClient>();
builder.Services.AddSingleton<FileMappingClient>();
builder.Services.AddSingleton<TitleFileClient>();
builder.Services.AddSingleton<TorrentFileClient>();
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<DownloadManagementService>();
builder.Services.AddSingleton<IDownloadSaver>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IEventStream>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IMediaFileRetriever>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IDownloadPatcher>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IDownloadRemover>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<ITitleLister>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<ITitleSaver>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<ITitleRemover>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddHostedService(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddScoped<GetMediaFileService>();
builder.Services.AddScoped<ListTitlesService>();
builder.Services.AddScoped<PatchDownloadService>();
builder.Services.AddScoped<RemoveDownloadService>();
builder.Services.AddScoped<RemoveTitleService>();
builder.Services.AddScoped<SaveDownloadService>();
builder.Services.AddScoped<SaveTitleService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<SubscribeService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.MapControllers();

var embeddedProvider = new ManifestEmbeddedFileProvider(typeof(Program).Assembly, "wwwroot");
app.UseFileServer(new FileServerOptions
{
    FileProvider = embeddedProvider,
});
app.MapFallbackToFile("movies/{*path}", "index.html", new StaticFileOptions { FileProvider = embeddedProvider });
app.MapFallbackToFile("series/{*path}", "index.html", new StaticFileOptions { FileProvider = embeddedProvider });
app.MapFallbackToFile("playlist/{*path}", "index.html", new StaticFileOptions { FileProvider = embeddedProvider });

System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
    FileName = "http://localhost:1713",
    UseShellExecute = true
});

app.Run();
