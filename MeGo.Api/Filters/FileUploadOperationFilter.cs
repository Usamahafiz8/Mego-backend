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
                var parameters = context.MethodInfo.GetParameters();
                
                // Remove existing parameters that are [FromForm] since we'll add them to RequestBody
                operation.Parameters = operation.Parameters?
                    .Where(p => !parameters.Any(param => 
                        param.Name == p.Name && 
                        param.GetCustomAttributes(typeof(FromFormAttribute), false).Any()))
                    .ToList() ?? new List<OpenApiParameter>();
                
                // Check for DTO with IFormFile properties
                var dtoParam = parameters.FirstOrDefault(p => 
                    p.GetCustomAttributes(typeof(FromFormAttribute), false).Any() &&
                    p.ParameterType.IsClass && 
                    p.ParameterType != typeof(string) &&
                    p.ParameterType != typeof(IFormFile));
                
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
                    // Handle individual [FromForm] parameters (including direct IFormFile)
                    var formParams = parameters.Where(p => p.GetCustomAttributes(typeof(FromFormAttribute), false).Any()).ToList();
                    if (formParams.Any())
                    {
                        var schemaProperties = new Dictionary<string, OpenApiSchema>();
                        
                        foreach (var param in formParams)
                        {
                            var paramType = param.ParameterType;
                            var isFile = paramType == typeof(IFormFile) || 
                                        (paramType.IsGenericType && 
                                         paramType.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                                         paramType.GetGenericArguments()[0] == typeof(IFormFile));
                            
                            schemaProperties[param.Name ?? "file"] = isFile
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
                }
            }
        }
    }
}
