using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Hangfire;

namespace CommerceHub.ServiceDefaults;

public static class Extensions
{
    public static WebApplicationBuilder ConfigureServiceDefaults(this WebApplicationBuilder builder, string serviceName)
    {
        builder.ConfigureOpenTelemetry(serviceName);
        builder.Services.AddServiceDefaultsHealthChecks();
        builder.Services.AddConfiguredHealthChecks(builder.Configuration);
        return builder;
    }

    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddSource("MassTransit")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName)
                        .AddEnvironmentVariableDetector())
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTLP_EXPORTER_ENDPOINT") ?? "http://localhost:4317");
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            });

        return builder;
    }

    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341");

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
        return builder;
    }

    public static IServiceCollection AddServiceDefaultsHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }

    public static IServiceCollection AddConfiguredHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var registeredNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var mysqlConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(mysqlConnectionString) && registeredNames.Add("mysql"))
        {
            services.AddHealthChecks().AddMySql(mysqlConnectionString, name: "mysql", tags: new[] { "ready", "database" });
        }

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString) && registeredNames.Add("redis"))
        {
            services.AddHealthChecks().AddRedis(redisConnectionString, name: "redis", tags: new[] { "ready", "cache" });
        }

        var elasticsearchUri = Environment.GetEnvironmentVariable("ELASTICSEARCH_URI") ?? configuration["Elasticsearch:Uri"];
        if (!string.IsNullOrWhiteSpace(elasticsearchUri) && registeredNames.Add("elasticsearch"))
        {
            services.AddHealthChecks().AddElasticsearch(elasticsearchUri, name: "elasticsearch", tags: new[] { "ready", "search" });
        }

        var rabbitMqConnectionString = configuration.GetSection("RabbitMQ")?.GetValue<string>("ConnectionString")
            ?? Environment.GetEnvironmentVariable("RabbitMQ__ConnectionString");
        if (!string.IsNullOrWhiteSpace(rabbitMqConnectionString) && registeredNames.Add("rabbitmq"))
        {
            services.AddHealthChecks().AddRabbitMQ(rabbitMqConnectionString, name: "rabbitmq", tags: new[] { "ready", "messaging" });
        }

        return services;
    }

    public static WebApplication MapServiceDefaultsHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            AllowCachingResponses = false
        });
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true
        });
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false
        });
        return app;
    }

    public static WebApplicationBuilder ConfigureHangfire(this WebApplicationBuilder builder, string serviceName)
    {
        var enabled = Environment.GetEnvironmentVariable("ENABLE_HANGFIRE") ?? "false";

        if (bool.TryParse(enabled, out var isEnabled) && isEnabled)
        {
            builder.Services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseInMemoryStorage();
            });

            builder.Services.AddHangfireServer(options =>
            {
                options.ServerName = $"{serviceName}-{Environment.MachineName}";
                options.WorkerCount = Math.Max(1, Environment.ProcessorCount);
                options.Queues = new[] { "default", serviceName.ToLowerInvariant() };
            });
        }

        return builder;
    }

    public static WebApplication UseHangfireDashboard(this WebApplication app, string path = "/hangfire")
    {
        var enabled = Environment.GetEnvironmentVariable("ENABLE_HANGFIRE") ?? "false";
        if (bool.TryParse(enabled, out var isEnabled) && isEnabled)
        {
            app.UseHangfireDashboard(path, new DashboardOptions
            {
                DashboardTitle = "CommerceHub Jobs",
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
        }
        return app;
    }

    public static IHttpClientBuilder AddResiliencePipeline(this IHttpClientBuilder client)
    {
        client.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(200);
            options.Retry.MaxDelay = TimeSpan.FromSeconds(10);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.2;
            options.CircuitBreaker.MinimumThroughput = 10;
        });
        return client;
    }
}
