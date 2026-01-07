using System.Net;
using System.Text.Json;
using MeGo.Api.Models.Responses;

namespace MeGo.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. RequestId: {RequestId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request.";
        var details = (string?)null;

        switch (exception)
        {
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                message = "You are not authorized to perform this action.";
                break;
            case ArgumentNullException argEx:
                code = HttpStatusCode.BadRequest;
                message = "Invalid request. Missing required parameter.";
                details = argEx.ParamName;
                break;
            case ArgumentException argEx:
                code = HttpStatusCode.BadRequest;
                message = "Invalid request.";
                details = argEx.Message;
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                message = "The requested resource was not found.";
                break;
            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                message = "Invalid operation.";
                break;
            case Microsoft.EntityFrameworkCore.DbUpdateException dbEx:
                code = HttpStatusCode.BadRequest;
                message = "Database operation failed.";
                details = dbEx.InnerException?.Message;
                break;
            case TimeoutException:
                code = HttpStatusCode.RequestTimeout;
                message = "The request timed out.";
                break;
        }

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Error = details ?? exception.Message,
            RequestId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
