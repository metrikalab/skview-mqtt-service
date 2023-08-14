
namespace skview_kepware_influx_service.MqttFiles
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private string _test1 = "cliente 1";
        private iMqttConect _iMqttConnect;

        public Worker(ILogger<Worker> logger, iMqttConect iMqttConnect)
        {
            _logger = logger;
            _iMqttConnect = iMqttConnect;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
                Console.WriteLine("thes");
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //await Task.Delay(1000, stoppingToken);
                await _iMqttConnect.Connect(_test1);

            
        }
    }
}