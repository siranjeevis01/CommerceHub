using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CommerceHub.ServiceDefaults;

public static partial class Extensions
{
    public static WebApplicationBuilder ConfigureHangfire(this WebApplicationBuilder builder, string serviceName)
    {
        var hangfireSection = builder.Configuration.GetSection("Hangfire");
        var enabled = hangfireSection["Enabled"] ?? Environment.GetEnvironmentVariable("ENABLE_HANGFIRE") ?? "false";

        if (bool.TryParse(enabled, out var isEnabled) && isEnabled)
        {
            var storageType = hangfireSection["StorageType"] ?? "MySQL";

            if (storageType.Equals("Redis", StringComparison.OrdinalIgnoreCase))
            {
                var redisConn = builder.Configuration.GetSection("Redis")["ConnectionString"]
                    ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");

                if (!string.IsNullOrWhiteSpace(redisConn))
                {
                    builder.Services.AddHangfire(config =>
                    {
                        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings()
                            .UseInMemoryStorage();
                    });
                }
            }
            else
            {
                var dbConn = builder.Configuration.GetSection("ConnectionStrings")["Order"]
                    ?? Environment.GetEnvironmentVariable("ORDER_DB_CONNECTION");

                if (!string.IsNullOrWhiteSpace(dbConn))
                {
                    builder.Services.AddHangfire(config =>
                    {
                        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings()
                            .UseInMemoryStorage();
                    });
                }
            }

            builder.Services.AddHangfireServer(options =>
            {
                options.ServerName = $"{serviceName}-{Environment.MachineName}";
                options.WorkerCount = Math.Max(1, Environment.ProcessorCount);
                options.Queues = ["default", serviceName.ToLowerInvariant()];
            });

            Log.Information("Hangfire configured with {StorageType} storage for {ServiceName}", storageType, serviceName);
        }

        return builder;
    }

    public static WebApplication UseHangfireDashboard(this WebApplication app, string path = "/hangfire")
    {
        var hangfireSection = app.Services.GetRequiredService<IConfiguration>().GetSection("Hangfire");
        var enabled = hangfireSection["Enabled"] ?? Environment.GetEnvironmentVariable("ENABLE_HANGFIRE") ?? "false";
        if (bool.TryParse(enabled, out var isEnabled) && isEnabled)
        {
            app.UseHangfireDashboard(path, new DashboardOptions
            {
                DashboardTitle = "CommerceHub Jobs",
                Authorization = [new HangfireAuthorizationFilter()]
            });
        }
        return app;
    }

    public static WebApplicationBuilder ConfigureServiceDefaults(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();
        return builder;
    }

    public static WebApplication MapServiceDefaultsHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new() { Predicate = _ => true });
        return app;
    }
}
