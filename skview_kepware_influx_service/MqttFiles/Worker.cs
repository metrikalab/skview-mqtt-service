namespace skview_kepware_influx_service.MqttFiles
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly iMqttConect _iMqttConnect;

    public Worker(ILogger<Worker> logger, iMqttConect iMqttConnect)
    {
      _logger = logger;
      _iMqttConnect = iMqttConnect;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      try
      {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //await Task.Delay(1000, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
          await _iMqttConnect.StartOperation();
        }
      }
      catch (OperationCanceledException)
      {

      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{Message}", ex.Message);
        Environment.Exit(1);

      ;
      }

    }
  }
}