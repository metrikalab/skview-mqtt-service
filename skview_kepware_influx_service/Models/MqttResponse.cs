namespace skview_kepware_influx_service.Models;

public class MqttResponse
{
  public long? Timestamp { get; set; }
  public List<MeasureValues>? Values { get; set; }
}