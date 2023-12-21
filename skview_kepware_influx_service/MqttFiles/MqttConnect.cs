﻿using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System.Text;
using MQTTnet.Formatter;
using skview_kepware_influx_service.Dtos;
using skview_kepware_influx_service.Models;

namespace skview_kepware_influx_service.MqttFiles
{
  public class MqttConnect : iMqttConect
  {
    private readonly IConfiguration _configuration;

    public MqttConnect(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public async Task StartOperation()
    {
      var mqttFactory = new MqttFactory();
      double operacionLoteValue = 0;
      double autotanqueLoteValue = 0;
      bool canStartOperation = false;
      using var mqttClient = mqttFactory.CreateMqttClient();
      var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_configuration["MqttServerIp"], 1883)
        .WithProtocolVersion(MqttProtocolVersion.V500).Build();

      mqttClient.DisconnectedAsync += async e =>
      {
        Console.WriteLine("Disconnected from MQTT broker.");
        // Aquí puedes añadir lógica para manejar la reconexión o registrar el evento.
        // Por ejemplo, puedes intentar reconectar aquí o establecer una bandera para manejar la reconexión en otro lugar del código.
      };
      
      mqttClient.ApplicationMessageReceivedAsync += async e =>
      {
        Console.WriteLine("Received application message.");
        var msgFromMQttBroker = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        var mqttResponse = JsonConvert.DeserializeObject<MqttResponse>(msgFromMQttBroker) ?? throw new InvalidOperationException();

        foreach (var item in mqttResponse.Values)
        {
          if (item is { Id: "ASFALTO.UCL31.BANDERA.OPERACION_LOTE", V: < 1000000 and > 0 })
          {
            Console.WriteLine(item.V);
            operacionLoteValue = (double)item.V;
            Console.WriteLine("Se puede iniciar una operación");
            await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.DISPLAY", 2);
            Thread.Sleep(1000);
            await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.BANDERACOMANDOS", 28);
            canStartOperation = true;
          }

          if (item is { Id: "ASFALTO.UCL31.BANDERA.AUTOTANQUE_LOTE", V: < 100000 and > 0 } && canStartOperation )
          {
            Console.WriteLine($"OperacionLote: {operacionLoteValue}");
            Console.WriteLine($"AutotanqueLote: {item.V}");
            //TODO: Validar con la base de datos que estos valores existan
              
            await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.BANDERACOMANDOS", 5);
            Thread.Sleep(1000);
            await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.RECETA_W", 1);
            Thread.Sleep(1000);
            await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.BANDERACOMANDOS", 6);
            Thread.Sleep(1000);
            await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.BANDERACOMANDOS", 10);
            Thread.Sleep(1000);
            await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.PRESET_ESCR", 9999);
            Thread.Sleep(1000);
            await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.BANDERACOMANDOS", 10);
          }
          
          if (item.Id == "ASFALTO.UCL31.BANDERA.ESTATUS_LOTE_INST" && canStartOperation)
          {
            Console.WriteLine($"Debería ser 13: {item.V}");
            if (item.V is 13)
            {
              GetDataRestDataFromUcl();
              // PostDataToDatabase(dataToInsert);
              //TODO: Escribir datos actuales (tal vez requiera rest)
              
              await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.BANDERACOMANDOS", 7);
              Thread.Sleep(1000);
              await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.BANDERACOMANDOS", 56);
              Thread.Sleep(1000);
              await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.DISPLAY", 1);
              Thread.Sleep(1000);
              await PublishToUcl("ucl31_bandera/write","ASFALTO.UCL31.BANDERA.BANDERACOMANDOS", 28);
              operacionLoteValue = 0;
              canStartOperation = false;
            }
          }
        }

        var msgTopic = e.ApplicationMessage.Topic;
        Console.WriteLine($"Received message on topic {msgTopic}");

        // return Task.CompletedTask;
      };
      
      
      

      await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
      Console.WriteLine("Se conectó");

      var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
        .WithTopicFilter(
          f => { f.WithTopic("ucl31_bandera"); })
        .Build();

      await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

      Console.WriteLine("MQTT client subscribed to topic.");

      Console.WriteLine("Press enter to exit.");
      Console.ReadLine();
    }
    
    public async Task PublishToUcl(string topic, string tagName, double value)
    {
      var mqttFactory = new MqttFactory();
      var mqttJsonObject = new
      {
        id = tagName,
        v = value
      };
      var mqttString = JsonConvert.SerializeObject(new[] { mqttJsonObject });


      using var mqttClient = mqttFactory.CreateMqttClient();
      var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithTcpServer(_configuration["MqttServerIp"], 1883)
        .WithProtocolVersion(MqttProtocolVersion.V500)
        .Build();

      await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

      var applicationMessage = new MqttApplicationMessageBuilder()
        .WithTopic(topic)
        .WithPayload(mqttString)
        // .WithPayload("[{\"id\": \"ASFALTO.UCL31.BANDERA.DISPLAY\",\"v\": 2}]")
        .Build();

      await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

      await mqttClient.DisconnectAsync();
            
      Console.WriteLine($"MQTT application message is published: {value}.");
    }

    private async void GetDataRestDataFromUcl()
    {
      var idsList = new List<string>
      {
        "ASFALTO.UCL31.LOTE.DENS_OBS_LOTE",
        "ASFALTO.UCL31.LOTE.DENS_CORR_LOTE",
        "ASFALTO.UCL31.LOTE.PRES_LOTE",
        "ASFALTO.UCL31.LOTE.TEMP_LOTE",
        "ASFALTO.UCL31.LOTE.MF_LOTE",
        "ASFALTO.UCL31.LOTE.KFACTOR_LOTE",
        "ASFALTO.UCL31.LOTE.CTL_LOTE",
        "ASFALTO.UCL31.LOTE.CPL_LOTE",
        "ASFALTO.UCL31.LOTE.CCF_LOTE",
        "ASFALTO.UCL31.LOTE.VOLNAT_LOTE",
        "ASFALTO.UCL31.LOTE.VOLCORR_LOTE",
        "ASFALTO.UCL31.LOTE.MAS_LOTE",
        "ASFALTO.UCL31.LOTE.OPERACION_LOTE",
        "ASFALTO.UCL31.LOTE.NUM_TRANS_LOTE",
        "ASFALTO.UCL31.LOTE.FLUJO_LOTE",
        "ASFALTO.UCL31.LOTE.FECHA_INI_LOTE",
        "ASFALTO.UCL31.LOTE.FECHA_FIN_LOTE",
        "ASFALTO.UCL31.LOTE.AUTOTANQUE_LOTE"
      };
      
      var values = new Dictionary<string, double?>();
      
      var idsPart = string.Join("&", idsList.ConvertAll(id => $"ids={id}"));
      var apiUrl = _configuration["RestApiServerIp"] + $"?{idsPart}";

      //Connect to the server (kepserver) for retrieving the information
      var httpClient = new HttpClient();
      var batchFillerOperationUpdated = new BatchFillerOperationUpdateFromUclDto
      {
        OperationNumber = "ooooo",
        TankTruckNumberPg = "oooo",
        OperationStatusId = new Guid("5EC42E4E-1FA7-4A4C-A918-0458DB1732F7"),
        StartDateByUcl = DateTime.Now,
        EndDateByUcl = DateTime.Now,
        BatchNumber = 199,
        Weight = 0,
        Density = 0,
        Temperature = 0,
        Pressure = 0,
        GrossVolume = 0,
        NetVolume = 0,
        BaseDensity = 0,
        MeterFactor = 0,
        KFactor = 0,
        CTL = 0,
        CPL = 0,
        CCF = 0,
        Flux = 0
      };

      try
      {
        var response = await httpClient.GetAsync(apiUrl);
        if (response.IsSuccessStatusCode)
        {
          var jsonResult = await response.Content.ReadAsStringAsync();
          var jsonObject = JsonConvert.DeserializeObject<RestResponse>(jsonResult);
          // foreach (var result in jsonObject.readResults)
          // {
          //   var density = result.Id == "ASFALTO.UCL31.LOTE.DENS_OBS_LOTE" ? result.V : null;
          //   var baseDensity = result.Id == "ASFALTO.UCL31.LOTE.DENS_CORR_LOTE" ? result.V : null;
          //   var pressure = result.Id == "ASFALTO.UCL31.LOTE.PRES_LOTE" ? result.V : null;
          //   Console.WriteLine($"ID: {result.Id}, Valor: {result.V}");
          // }
          
          foreach (var result in jsonObject.readResults)
          {
            foreach (var id in idsList)
            {
              if (result.Id == id)
              {
                values[id] = result.V;
                break;
              }
            }
          }
          
          var batchFillerOperationUpdate = new BatchFillerOperationUpdateFromUclDto
          {
            OperationNumber = "teeeestUCL",
            TankTruckNumberPg = "UCLTesttt",
            OperationStatusId = new Guid("5EC42E4E-1FA7-4A4C-A918-0458DB1732F7"),
            StartDateByUcl = DateTime.Now,
            EndDateByUcl = DateTime.Now,
            BatchNumber = 199,
            Weight = 0,
            Density = 0,
            Temperature = 0,
            Pressure = 0,
            GrossVolume = 0,
            NetVolume = 0,
            BaseDensity = 0,
            MeterFactor = 0,
            KFactor = 0,
            CTL = 0,
            CPL = 0,
            CCF = 0,
            Flux = 0
          };
      
          foreach (var pair in values)
          {
            if (pair.Key == "ASFALTO.UCL31.LOTE.DENS_OBS_LOTE")
            {
              batchFillerOperationUpdate.Density = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.DENS_CORR_LOTE")
            {
              batchFillerOperationUpdate.BaseDensity = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.PRES_LOTE")
            {
              batchFillerOperationUpdate.Pressure = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.TEMP_LOTE")
            {
              batchFillerOperationUpdate.Temperature = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.MF_LOTE")
            {
              batchFillerOperationUpdate.MeterFactor = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.KFACTOR_LOTE")
            {
              batchFillerOperationUpdate.KFactor = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.CTL_LOTE")
            {
              batchFillerOperationUpdate.CTL = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.CPL_LOTE")
            {
              batchFillerOperationUpdate.CPL = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.CCF_LOTE")
            {
              batchFillerOperationUpdate.CCF = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.VOLNAT_LOTE")
            {
              batchFillerOperationUpdate.GrossVolume = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.VOLCORR_LOTE")
            {
              batchFillerOperationUpdate.NetVolume = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.MAS_LOTE")
            {
              batchFillerOperationUpdate.Weight = pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.OPERACION_LOTE")
            {
              batchFillerOperationUpdate.OperationNumber = pair.Value?.ToString();
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.NUM_TRANS_LOTE")
            {
              batchFillerOperationUpdate.BatchNumber = (int?)pair.Value;
            }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.FLUJO_LOTE")
            {
              batchFillerOperationUpdate.Flux = pair.Value;
            }
            // else if (pair.Key == "ASFALTO.UCL31.LOTE.FECHA_INI_LOTE")
            // {
            //   batchFillerOperationUpdate.StartDateByUcl = pair.Value;
            // }
            // else if (pair.Key == "ASFALTO.UCL31.LOTE.FECHA_FIN_LOTE")
            // {
            //   batchFillerOperationUpdate.EndDateByUcl = pair.Value;
            // }
            else if (pair.Key == "ASFALTO.UCL31.LOTE.AUTOTANQUE_LOTE")
            {
              batchFillerOperationUpdate.TankTruckNumberPg = pair.Value?.ToString();
            }
          }
      
          Console.WriteLine(batchFillerOperationUpdate);
          PostDataToDatabase(batchFillerOperationUpdate);
        }
        
      }
      catch (HttpRequestException e)
      {
        //TODO: Definir el estatus que se pondrá para controlar el estatus de fallo.
        Console.WriteLine(e.Message);
      }
    }

    private static async void PostDataToDatabase(BatchFillerOperationUpdateFromUclDto test)
    {
      
      
      string jsonData = JsonConvert.SerializeObject(test);

      // URL del endpoint
      string apiUrl = "http://localhost:5001/api/BatchFillerOperations/UpdateBatchFillerOperationOperationFromUcl/9A39372B-745E-45AB-83F0-A2DE1CF5CC94";

      // Crear una instancia de HttpClient (se puede reutilizar a lo largo de la aplicación)
      using var httpClient = new HttpClient();

      try
      {
        // Configurar el encabezado de la solicitud para indicar JSON
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

        // Crear una solicitud POST con los datos JSON
        HttpResponseMessage response = await httpClient.PutAsync(apiUrl, new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json"));

        // Verificar si la solicitud fue exitosa (código de estado 200-299)
        if (response.IsSuccessStatusCode)
        {
          // Leer la respuesta
          string responseBody = await response.Content.ReadAsStringAsync();
          Console.WriteLine("La solicitud fue exitosa:");
          Console.WriteLine(responseBody);
        }
        else
        {
          Console.WriteLine($"Error: {response.StatusCode}");
        }
      }
      catch (HttpRequestException e)
      {
        Console.WriteLine($"Error en la solicitud HTTP: {e.Message}");
      }
    }
  }
} 