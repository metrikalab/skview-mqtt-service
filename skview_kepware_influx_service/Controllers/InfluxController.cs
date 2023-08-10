using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Writes;
using Microsoft.AspNetCore.Mvc;
using skview_kepware_influx_service.Data;

namespace skview_kepware_influx_service.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class InfluxController : Controller
  {
    private readonly InfluxDBClient _influxDbClient;
    private readonly string _bucket;
    private readonly string _org;

    public InfluxController(InfluxDBConnection influxDBConnection)
    {
      _influxDbClient = influxDBConnection.GetInfluxDbClient();
      _bucket = influxDBConnection.Bucket;
      _org = influxDBConnection.Org;
    }

    [HttpPost]
    public IActionResult WriteData([FromBody] string measurement, double densidad, double flujo, double presion, double temperaturaInstantanea)
    {

      var point = PointData
          .Measurement(measurement)
          .Field("Densidad", densidad)
          .Field("Flujo", flujo)
          .Field("Presion", presion)
          .Field("TemperaturaInstantanea", temperaturaInstantanea);

      Console.WriteLine(point.ToString());

      using (var writeApi = _influxDbClient.GetWriteApi())
      {
        writeApi.WritePoint(point, _bucket, _org);
      }

      return Ok("Datos escritos en InfluxDB.");
    }

    [HttpGet]
    public async Task<List<FluxTable>> GetData()
    {
      var queryApi = _influxDbClient.GetQueryApi();
      var query = "from(bucket:\"SKViewData\") |> range(start: -30d, stop: now()) |> filter(fn: (r) => r._measurement == \"PA-07-ULC-3007\")  |> filter(fn: (r) => r._field == \"Densidad\" or r._field == \"Flujo\" or r._field == \"Presion\" or r._field == \"TemperaturaInstantanea\")";
      var tables = await _influxDbClient.GetQueryApi().QueryAsync(query, _org);
      return tables;

    }
  }
}
