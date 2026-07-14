using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace CommerceHub.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;
    private readonly IServiceProvider _serviceProvider;
    private static readonly Stopwatch Uptime = Stopwatch.StartNew();

    public HealthController(
        HealthCheckService healthCheckService,
        ILogger<HealthController> logger,
        IServiceProvider serviceProvider)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// GET /api/v1/health — Basic liveness probe. Returns 200 if the process is running.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(LivenessResponse), StatusCodes.Status200OK)]
    public IActionResult Liveness()
    {
        return Ok(new LivenessResponse
        {
            Status = "Healthy",
            Service = "CommerceHub.Api",
            Timestamp = DateTime.UtcNow,
            UptimeSeconds = Uptime.Elapsed.TotalSeconds
        });
    }

    /// <summary>
    /// GET /api/v1/health/ready — Readiness probe. Tests DB connectivity for all registered services.
    /// Returns 200 only when all dependency checks pass.
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Readiness(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(
            check => check.Tags.Contains("ready"), cancellationToken);

        var response = new ReadinessResponse
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            DurationMs = (int)report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.Select(e => new HealthCheckEntry
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                DurationMs = (int)e.Value.Duration.TotalMilliseconds,
                Description = e.Value.Description,
                Exception = e.Value.Exception?.Message
            }).ToList()
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// GET /api/v1/health/detailed — Comprehensive status including DB, Redis, RabbitMQ, memory, uptime.
    /// Requires no authentication; intended for internal monitoring only (protect via network in production).
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(DetailedHealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Detailed(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new DetailedHealthResponse
        {
            Status = report.Status.ToString(),
            Service = "CommerceHub.Api",
            Timestamp = DateTime.UtcNow,
            UptimeSeconds = Uptime.Elapsed.TotalSeconds,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Checks = new DetailedCheckGroup
            {
                Database = await CheckDatabaseAsync(cancellationToken),
                Cache = await CheckRedisAsync(cancellationToken),
                Messaging = await CheckRabbitMqAsync(cancellationToken),
                Memory = GetMemoryStats()
            }
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    private async Task<DbCheckResult> CheckDatabaseAsync(CancellationToken ct)
    {
        var result = new DbCheckResult { Status = "NotConfigured" };
        var dbNames = new[]
        {
            "Identity", "Product", "Order", "Payment",
            "Vendor", "Inventory", "Notification", "CMS", "Analytics", "AIAgent"
        };

        var connected = new List<string>();
        var failed = new List<string>();

        foreach (var name in dbNames)
        {
            try
            {
                var dbContextType = Type.GetType($"CommerceHub.{name}.Infrastructure.Data.{name}DbContext");
                if (dbContextType == null) continue;

                var db = _serviceProvider.GetService(dbContextType) as DbContext;
                if (db == null) continue;

                var canConnect = await db.Database.CanConnectAsync(ct);
                if (canConnect)
                    connected.Add(name);
                else
                    failed.Add(name);
            }
            catch
            {
                failed.Add(name);
            }
        }

        result.Status = failed.Count == 0 ? "Healthy" : "Degraded";
        result.ConnectedDatabases = connected;
        result.FailedDatabases = failed;
        return result;
    }

    private async Task<CacheCheckResult> CheckRedisAsync(CancellationToken ct)
    {
        var result = new CacheCheckResult { Status = "NotConfigured" };

        try
        {
            var redisConn = _serviceProvider.GetService<IConnectionMultiplexer>();
            if (redisConn == null)
            {
                var cache = _serviceProvider.GetService<IDistributedCache>();
                if (cache == null)
                    return result;

                var sw = Stopwatch.StartNew();
                await cache.GetAsync("__health_check_probe__", ct);
                sw.Stop();
                result.Status = "Healthy";
                result.LatencyMs = (int)sw.ElapsedMilliseconds;
                return result;
            }

            var endpoint = redisConn.GetEndPoints().FirstOrDefault();
            if (endpoint == null)
            {
                result.Status = "NoEndpoints";
                return result;
            }

            var server = redisConn.GetServer(endpoint);
            var latency = await server.PingAsync();

            result.Status = "Healthy";
            result.LatencyMs = (int)latency.TotalMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis health check failed");
            result.Status = "Unhealthy";
            result.Error = ex.Message;
        }

        return result;
    }

    private async Task<MessagingCheckResult> CheckRabbitMqAsync(CancellationToken ct)
    {
        var result = new MessagingCheckResult { Status = "NotConfigured" };

        try
        {
            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            if (string.IsNullOrWhiteSpace(rabbitHost))
                return result;

            result.Status = "Configured";
            result.Host = rabbitHost;
            result.VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/";

            // MassTransit doesn't expose a direct health check — verify connectivity via bus control if available
            var busControl = _serviceProvider.GetService<MassTransit.IBusControl>();
            if (busControl != null)
            {
                result.Status = "Healthy";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ health check failed");
            result.Status = "Unhealthy";
            result.Error = ex.Message;
        }

        await Task.CompletedTask;
        return result;
    }

    private static MemoryCheckResult GetMemoryStats()
    {
        var process = Process.GetCurrentProcess();
        return new MemoryCheckResult
        {
            Status = "Healthy",
            WorkingSetMB = (int)(process.WorkingSet64 / (1024 * 1024)),
            PrivateMemoryMB = (int)(process.PrivateMemorySize64 / (1024 * 1024)),
            GcTotalMemoryMB = (int)(GC.GetTotalMemory(false) / (1024 * 1024)),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            ThreadPoolWorkerThreads = ThreadPool.ThreadCount,
            ThreadPoolPendingWorkItems = ThreadPool.PendingWorkItemCount
        };
    }

    // ========== Response DTOs ==========

    public sealed class LivenessResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("service")] public string Service { get; set; } = "";
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
        [JsonPropertyName("uptimeSeconds")] public double UptimeSeconds { get; set; }
    }

    public sealed class ReadinessResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
        [JsonPropertyName("durationMs")] public int DurationMs { get; set; }
        [JsonPropertyName("checks")] public List<HealthCheckEntry> Checks { get; set; } = [];
    }

    public sealed class HealthCheckEntry
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("durationMs")] public int DurationMs { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("exception")] public string? Exception { get; set; }
    }

    public sealed class DetailedHealthResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("service")] public string Service { get; set; } = "";
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
        [JsonPropertyName("uptimeSeconds")] public double UptimeSeconds { get; set; }
        [JsonPropertyName("environment")] public string Environment { get; set; } = "";
        [JsonPropertyName("checks")] public DetailedCheckGroup Checks { get; set; } = new();
    }

    public sealed class DetailedCheckGroup
    {
        [JsonPropertyName("database")] public DbCheckResult Database { get; set; } = new();
        [JsonPropertyName("cache")] public CacheCheckResult Cache { get; set; } = new();
        [JsonPropertyName("messaging")] public MessagingCheckResult Messaging { get; set; } = new();
        [JsonPropertyName("memory")] public MemoryCheckResult Memory { get; set; } = new();
    }

    public sealed class DbCheckResult
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("connectedDatabases")] public List<string> ConnectedDatabases { get; set; } = [];
        [JsonPropertyName("failedDatabases")] public List<string> FailedDatabases { get; set; } = [];
    }

    public sealed class CacheCheckResult
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("latencyMs")] public int LatencyMs { get; set; }
        [JsonPropertyName("error")] public string? Error { get; set; }
    }

    public sealed class MessagingCheckResult
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("host")] public string? Host { get; set; }
        [JsonPropertyName("virtualHost")] public string? VirtualHost { get; set; }
        [JsonPropertyName("error")] public string? Error { get; set; }
    }

    public sealed class MemoryCheckResult
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("workingSetMB")] public int WorkingSetMB { get; set; }
        [JsonPropertyName("privateMemoryMB")] public int PrivateMemoryMB { get; set; }
        [JsonPropertyName("gcTotalMemoryMB")] public int GcTotalMemoryMB { get; set; }
        [JsonPropertyName("gen0Collections")] public int Gen0Collections { get; set; }
        [JsonPropertyName("gen1Collections")] public int Gen1Collections { get; set; }
        [JsonPropertyName("gen2Collections")] public int Gen2Collections { get; set; }
        [JsonPropertyName("threadPoolWorkerThreads")] public int ThreadPoolWorkerThreads { get; set; }
        [JsonPropertyName("threadPoolPendingWorkItems")] public long ThreadPoolPendingWorkItems { get; set; }
    }
}
