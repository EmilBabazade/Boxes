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
// normally i would configure dataContext here, but since its just a local db i just put the config in the dataContext class
builder.Services.AddDbContext<DataContext>();

var host = builder.Build();
host.Run();
