using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skview_kepware_influx_service.MqttFiles
{
    public class MqttConnect : iMqttConect
    {
        
        public async Task Connect(string Test1)
        {
            

            Console.WriteLine("prueba");
            var msgFromMQTTBroker = "";
            var msgTopic = "";
            
            //JsonObject dataMQTT = new JsonObject();

            var mqttFactory = new MqttFactory();
            IMqttClient mqttClient = mqttFactory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                                .WithClientId(Guid.NewGuid().ToString())
                                .WithWebSocketServer("ws://localhost:9001")
                                .WithCleanSession(true)
                                .Build();

            // Connect to MQTT broker
            var connectResult = await mqttClient.ConnectAsync(options);
            var topicFilter = new MqttTopicFilterBuilder()
                                    .WithTopic("iotgateway")
                                    .Build();
            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Service connect to Broker");
            }
            try
            {
                await mqttClient.SubscribeAsync(topicFilter);
                Console.WriteLine("Subscribe to " + topicFilter.Topic + " successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during connection because of {ex.Message}");
            }

            mqttClient.DisconnectedAsync += async e => {
                Console.WriteLine("Disconnected from MQTT broker.");
                await Task.Delay(5);
                try
                {
                    await mqttClient.ConnectAsync(options);
                    Console.WriteLine("MQTT broker online again");
                    await mqttClient.SubscribeAsync(topicFilter);
                    Console.WriteLine("Subscribe to topic " + topicFilter.Topic + " successful");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Reconnecting to MQTT broker failed, because of " + ex.Message);

                }
            };
            mqttClient.ApplicationMessageReceivedAsync += e => {
                msgFromMQTTBroker = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                msgTopic = e.ApplicationMessage.Topic;
                JObject json = JObject.Parse(msgFromMQTTBroker);
                Console.WriteLine($"Received message on topic {msgTopic}");

                long msgTime = Convert.ToInt64(json.GetValue("timestamp"));
                if (msgTime != 0)
                {
                    DateTime TimeSt_get = DateTimeOffset.FromUnixTimeMilliseconds(msgTime).DateTime.ToLocalTime();
                    Console.WriteLine(TimeSt_get.ToShortTimeString().ToString());
                    json["timestamp"] = TimeSt_get;
                }
                using (var sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\MqttFiles\Files\test1.txt"))
                {
                    Task.Delay(1000);
                    sw.WriteLineAsync(json.ToString());
                }
                Console.WriteLine(json.ToString());
                return Task.CompletedTask;
            };
        }
    }
}
