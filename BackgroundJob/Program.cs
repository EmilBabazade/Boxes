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
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite());

var host = builder.Build();
host.Run();
