using BackgroundJob;
using Data.AutomapperProfiles;
using Services.BoxProcessing;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IBoxProcessingService, BoxProcessingService>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<BoxMapperProfile>();
    cfg.AddProfile<itemMapperProfile>();
});

var host = builder.Build();
host.Run();
