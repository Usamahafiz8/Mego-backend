using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace MeGo.Api.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if method uses [Consumes("multipart/form-data")]
            var consumesAttribute = context.MethodInfo.GetCustomAttributes(typeof(ConsumesAttribute), false)
                .FirstOrDefault() as ConsumesAttribute;
            
            if (consumesAttribute?.ContentTypes?.Contains("multipart/form-data") == true)
            {
                // Check for DTO with IFormFile properties
                var parameters = context.MethodInfo.GetParameters();
                var dtoParam = parameters.FirstOrDefault(p => 
                    p.GetCustomAttributes(typeof(FromFormAttribute), false).Any() &&
                    p.ParameterType.IsClass && 
                    p.ParameterType != typeof(string));
                
                if (dtoParam != null)
                {
                    var dtoType = dtoParam.ParameterType;
                    var properties = dtoType.GetProperties();
                    var schemaProperties = new Dictionary<string, OpenApiSchema>();
                    
                    foreach (var prop in properties)
                    {
                        var propType = prop.PropertyType;
                        var isFile = propType == typeof(IFormFile) || 
                                    (propType.IsGenericType && 
                                     propType.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                                     propType.GetGenericArguments()[0] == typeof(IFormFile));
                        
                        schemaProperties[prop.Name] = isFile
                            ? new OpenApiSchema { Type = "string", Format = "binary" }
                            : new OpenApiSchema { Type = "string" };
                    }
                    
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["multipart/form-data"] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "object",
                                    Properties = schemaProperties
                                }
                            }
                        }
                    };
                }
                else
                {
                    // Fallback: handle individual [FromForm] parameters
                    var formParams = parameters.Where(p => p.GetCustomAttributes(typeof(FromFormAttribute), false).Any()).ToList();
                    if (formParams.Any())
                    {
                        operation.RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["multipart/form-data"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Type = "object",
                                        Properties = formParams.ToDictionary(
                                            p => p.Name ?? "",
                                            p =>
                                            {
                                                var paramType = p.ParameterType;
                                                var isFile = paramType == typeof(IFormFile) || 
                                                            (paramType.IsGenericType && 
                                                             paramType.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                                                             paramType.GetGenericArguments()[0] == typeof(IFormFile));
                                                
                                                return isFile
                                                    ? new OpenApiSchema { Type = "string", Format = "binary" }
                                                    : new OpenApiSchema { Type = "string" };
                                            }
                                        )
                                    }
                                }
                            }
                        };
                    }
                }
            }
        }
    }
}
