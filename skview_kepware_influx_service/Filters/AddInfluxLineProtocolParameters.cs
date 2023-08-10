using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace skview_kepware_influx_service.Filters
{
  public class AddInfluxLineProtocolParameters : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      if (context.MethodInfo.Name == "WriteData")
      {
        operation.Parameters.Clear();

        operation.Parameters.Add(new OpenApiParameter
        {
          Name = "measurement",
          In = ParameterLocation.Query,
          Required = true,
          Schema = new OpenApiSchema
          {
            Type = "string"
          }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
          Name = "densidad",
          In = ParameterLocation.Query,
          Required = true,
          Schema = new OpenApiSchema
          {
            Type = "number",
            Format = "double"
          }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
          Name = "flujo",
          In = ParameterLocation.Query,
          Required = true,
          Schema = new OpenApiSchema
          {
            Type = "number",
            Format = "double"
          }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
          Name = "presion",
          In = ParameterLocation.Query,
          Required = true,
          Schema = new OpenApiSchema
          {
            Type = "number",
            Format = "double"
          }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
          Name = "temperaturaInstantanea",
          In = ParameterLocation.Query,
          Required = true,
          Schema = new OpenApiSchema
          {
            Type = "number",
            Format = "double"
          }
        });
      }
    }
  }
}
