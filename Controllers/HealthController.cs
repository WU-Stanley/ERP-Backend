using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly WUIAMDbContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(WUIAMDbContext dbContext, ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("liveness")]
        public IActionResult Liveness()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow.ToString("o") });
        }

        [HttpGet("readiness")]
        public async Task<IActionResult> Readiness()
        {
            try
            {
                await _dbContext.Database.CanConnectAsync();
                return Ok(new
                {
                    status = "ready",
                    database = "connected",
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connectivity check failed");
                return StatusCode(503, new
                {
                    status = "not_ready",
                    database = "disconnected",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
        }

        [HttpGet("startup")]
        public IActionResult Startup()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "unknown";

            return Ok(new
            {
                status = "running",
                environment = env,
                version = version,
                uptime = Environment.TickCount64,
                timestamp = DateTime.UtcNow.ToString("o"),
                features = new[]
                {
                    "JWT Authentication",
                    "Hangfire Background Jobs",
                    "Audit Logging",
                    "Rate Limiting",
                    "Global Exception Handling",
                    "CORS Policy"
                }
            });
        }
    }
}
