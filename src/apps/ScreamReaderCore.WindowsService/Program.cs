// See https://aka.ms/new-console-template for more information

using App.WindowsService;
using App.WindowsService.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "ScreamReaderCore Windows Service";
});

LoggerProviderOptions.RegisterProviderOptions<
    EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddScreamReader();
builder.Services.AddHostedService<ScreamBackgroundService>();

IHost host = builder.Build();
host.Run();