using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeGo.Api.Data;
using MeGo.Api.Models.Responses;

namespace MeGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<HealthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthStatus>), 200)]
    public IActionResult Get()
    {
        return Ok(ApiResponse<HealthStatus>.SuccessResponse(new HealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        }, "Service is healthy"));
    }

    [HttpGet("detailed")]
    [ProducesResponseType(typeof(ApiResponse<DetailedHealthStatus>), 200)]
    public async Task<IActionResult> GetDetailed()
    {
        var health = new DetailedHealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown"
        };

        try
        {
            await _context.Database.CanConnectAsync();
            health.Database = new ComponentHealth { Status = "Healthy", ResponseTime = 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            health.Database = new ComponentHealth { Status = "Unhealthy", Error = ex.Message };
            health.Status = "Degraded";
        }

        var statusCode = health.Status == "Healthy" ? 200 : 503;
        return StatusCode(statusCode, ApiResponse<DetailedHealthStatus>.SuccessResponse(health, "Health check completed"));
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return canConnect ? Ok(new { status = "ready" }) : StatusCode(503, new { status = "not ready" });
        }
        catch
        {
            return StatusCode(503, new { status = "not ready" });
        }
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "alive" });
    }
}

public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
}

public class DetailedHealthStatus : HealthStatus
{
    public string Environment { get; set; } = string.Empty;
    public ComponentHealth Database { get; set; } = new();
}

public class ComponentHealth
{
    public string Status { get; set; } = string.Empty;
    public long ResponseTime { get; set; }
    public string? Error { get; set; }
}
