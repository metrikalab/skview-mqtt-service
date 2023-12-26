using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.OpenApi.Models;
using skview_kepware_influx_service.MqttFiles;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
  options.ServiceName = ".Skview_Mqtt_Service";
});

LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddHostedService<Worker>().AddSingleton<iMqttConect, MqttConnect>();

IHost host = builder.Build();
host.Run();