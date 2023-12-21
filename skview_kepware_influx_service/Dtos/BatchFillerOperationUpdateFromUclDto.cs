namespace skview_kepware_influx_service.Dtos
{
  public class BatchFillerOperationUpdateFromUclDto
  {
    public string? OperationNumber { get; set; }
    public string? TankTruckNumberPg { get; set; }
    public Guid? OperationStatusId { get; set; }
    public DateTime? StartDateByUcl { get; set; }
    public DateTime? EndDateByUcl { get; set; }
    public int? BatchNumber { get; set; }
    public double? Weight { get; set; }
    public double? Density { get; set; }
    public double? Temperature { get; set; }
    public double? Pressure { get; set; }
    public double? GrossVolume { get; set; }
    public double? NetVolume { get; set; }
    public double? BaseDensity { get; set; }
    public double? MeterFactor { get; set; }
    public double? KFactor { get; set; }
    public double? CTL { get; set; }
    public double? CPL { get; set; }
    public double? CCF { get; set; }
    public double? Flux { get; set; }
  }
}
