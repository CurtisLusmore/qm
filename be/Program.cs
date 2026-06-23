using be.HostedServices;
using be.Interfaces;
using be.ListDownloads;
using be.SaveDownload;
using be.Search;
using be.Subscribe;
using FifteenthStandard.Storage;
using FifteenthStandard.Storage.File;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddControllers();
builder.Services.AddScoped<ListDownloadsService>();
builder.Services.AddScoped<SaveDownloadService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<SubscribeService>();
builder.Services.AddSingleton<IKeyValueStore>(new FileKeyValueStore("data", true));
builder.Services.AddSingleton(p => new DownloadManagementService(
    p.GetRequiredService<IHttpClientFactory>(),
    p.GetRequiredService<IKeyValueStore>(),
    "data",
    p.GetRequiredService<ILogger<DownloadManagementService>>()));
builder.Services.AddHostedService(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IDownloadLister>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IDownloadSaver>(p => p.GetRequiredService<DownloadManagementService>());

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
