using BackgroundJob;
using Services.BoxProcessing;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IBoxProcessingService, BoxProcessingService>();

var host = builder.Build();
host.Run();
