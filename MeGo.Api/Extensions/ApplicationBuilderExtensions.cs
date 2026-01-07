using MeGo.Api.Middleware;

namespace MeGo.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseProfessionalMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}
