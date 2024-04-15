using BackgroundJob;
using Data;
using Data.AutomapperProfiles;
using Microsoft.EntityFrameworkCore;
using Services.BoxProcessing;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IBoxProcessingService, BoxProcessingService>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<BoxMapperProfile>();
    cfg.AddProfile<itemMapperProfile>();
});

var folder = Environment.SpecialFolder.LocalApplicationData;
var dbPath = Path.Join(Environment.GetFolderPath(folder), "boxes.db");
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite($"Data Source={dbPath}"));

var host = builder.Build();
host.Run();
