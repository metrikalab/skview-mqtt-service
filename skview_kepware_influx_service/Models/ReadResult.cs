namespace skview_kepware_influx_service.Models;

public class ReadResult
{
  public string? Id { get; set; }
  public bool? S { get; set; }
  public string? R { get; set; }
  public double? V { get; set; }
  public long? T { get; set; }
}