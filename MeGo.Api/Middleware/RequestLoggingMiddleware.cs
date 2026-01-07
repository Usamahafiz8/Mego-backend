using System.Diagnostics;

namespace MeGo.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;

        _logger.LogInformation(
            "Incoming request: {Method} {Path} | RequestId: {RequestId} | IP: {IpAddress}",
            context.Request.Method,
            context.Request.Path,
            requestId,
            context.Connection.RemoteIpAddress?.ToString()
        );

        try
        {
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation(
                "Request completed: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Request failed: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId
            );
            throw;
        }
    }
}
