using be.GetCollection;
using be.GetMediaFile;
using be.HostedServices;
using be.Interfaces;
using be.ListDownloads;
using be.RemoveTitle;
using be.SaveDownload;
using be.SaveTitle;
using be.Search;
using be.Subscribe;

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
builder.Services.Configure<DownloadManagementService.Config>(builder.Configuration.GetSection("DownloadManagementService"));
builder.Services.AddSingleton<DownloadManagementService>();
builder.Services.AddSingleton<ICollectionRetriever>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IDownloadLister>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IDownloadSaver>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<IMediaFileRetriever>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<ITitleRemover>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddSingleton<ITitleSaver>(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddHostedService(p => p.GetRequiredService<DownloadManagementService>());
builder.Services.AddScoped<GetCollectionService>();
builder.Services.AddScoped<GetMediaFileService>();
builder.Services.AddScoped<ListDownloadsService>();
builder.Services.AddScoped<RemoveTitleService>();
builder.Services.AddScoped<SaveDownloadService>();
builder.Services.AddScoped<SaveTitleService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<SubscribeService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
