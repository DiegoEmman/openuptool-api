using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenUpTool.Api.Middleware;

/// <summary>
/// Filtro para manejar correctamente los file uploads en Swagger
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || 
                       p.ParameterType == typeof(IFormFile[]) ||
                       p.ParameterType == typeof(IEnumerable<IFormFile>) ||
                       (p.ParameterType.IsClass && 
                        p.ParameterType != typeof(string) && 
                        p.ParameterType.GetProperties().Any(prop => 
                            prop.PropertyType == typeof(IFormFile) ||
                            prop.PropertyType == typeof(IFormFile[]) ||
                            prop.PropertyType == typeof(IEnumerable<IFormFile>))))
            .ToList();

        if (!fileParameters.Any())
            return;

        // Limpiar parámetros existentes que causen conflicto
        operation.Parameters?.Clear();

        // Configurar como multipart/form-data
        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>(),
                        Required = new HashSet<string>()
                    }
                }
            }
        };

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

        // Agregar propiedades del modelo
        foreach (var param in fileParameters)
        {
            if (param.ParameterType == typeof(IFormFile))
            {
                schema.Properties[param.Name ?? "file"] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            else if (param.ParameterType.IsClass && param.ParameterType != typeof(string))
            {
                // Es un objeto con propiedades (como CreateArtifactVersionRequest)
                var properties = param.ParameterType.GetProperties();
                foreach (var prop in properties)
                {
                    if (prop.PropertyType == typeof(IFormFile))
                    {
                        schema.Properties[char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1)] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary",
                            Description = "Archivo a subir"
                        };
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        schema.Properties[char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1)] = new OpenApiSchema
                        {
                            Type = "string",
                            Description = prop.Name
                        };
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                    {
                        schema.Properties[char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1)] = new OpenApiSchema
                        {
                            Type = "integer",
                            Description = prop.Name
                        };
                    }
                    // Agregar más tipos según sea necesario
                }
            }
        }
    }
}
