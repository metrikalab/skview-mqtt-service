using Microsoft.OpenApi.Models;
using skview_kepware_influx_service.Data;
using skview_kepware_influx_service.Filters;
using skview_kepware_influx_service.MqttFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
  c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "InfluxAPI", Version = "v1" });
  c.OperationFilter<AddInfluxLineProtocolParameters>();
}
  );

string influxDbUrl = "";
string influxDbToken = "";
string influxDbOrg = "";
string influxDbBucket = "";


builder.Services.AddHostedService<Worker>().AddSingleton<iMqttConect, MqttConnect>();
builder.Services.AddSingleton(new InfluxDBConnection(influxDbUrl, influxDbToken, influxDbOrg, influxDbBucket));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();