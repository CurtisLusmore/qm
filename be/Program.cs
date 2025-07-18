using System.Reflection;
using Microsoft.Extensions.FileProviders;
using MonoTorrent;
using qm.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    });
}
builder.Services.AddControllers();
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("cors", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .WithMethods("GET", "POST", "PATCH", "DELETE")
            .WithHeaders("Content-Type");
    });
});

builder.Services.Configure<TorrentServiceConfig>(builder.Configuration.GetSection(TorrentServiceConfig.SectionName));
builder.Services.AddHttpClient();
builder.Services.AddScoped<SearchService>();
builder.Services.AddSingleton<TorrentService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TorrentService>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseFileServer(
    new FileServerOptions
    {
        FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetAssembly(typeof(Program))!, "wwwroot"),
    });
app.UseCors("cors");

app.Run();
