using InfluxDB.Client;

namespace skview_kepware_influx_service.Data
{
  public class InfluxDBConnection
  {
    private readonly string _url;
    private readonly string _token;
    private readonly string _org;
    private readonly string _bucket;

    public InfluxDBConnection(string url, string token, string org, string bucket)
    {
      _url = url;
      _token = token;
      _org = org;
      _bucket = bucket;
    }

    public InfluxDBClient GetInfluxDbClient()
    {
      return InfluxDBClientFactory.Create(_url, _token.ToCharArray());
    }

    public string Org => _org;
    public string Bucket => _bucket;
  }
}
