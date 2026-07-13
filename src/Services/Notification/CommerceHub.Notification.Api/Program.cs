using System.Text.RegularExpressions;
using CommerceHub.Notification.Api.Consumers;
using CommerceHub.Notification.Application;
using CommerceHub.Notification.Application.Consumers;
using CommerceHub.Notification.Infrastructure;
using CommerceHub.Notification.Infrastructure.Hubs;
using CommerceHub.ServiceDefaults;
using MassTransit;
using Prometheus;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

foreach (var key in builder.Configuration.AsEnumerable().Select(kv => kv.Key))
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value) && value.Contains("${"))
        builder.Configuration[key] = Regex.Replace(value, @"\${([^}]+)}", m => Environment.GetEnvironmentVariable(m.Groups[1].Value) ?? "");
}

builder.ConfigureServiceDefaults("CommerceHub-Notification");
builder.ConfigureHangfire("commercehub-notification");
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Redis for caching user notifications
var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConn; options.InstanceName = "CommerceHub_Notifications_"; });

// SignalR
builder.Services.AddSignalR().AddStackExchangeRedis(redisConn);

// Infrastructure services (Email, SMS, WhatsApp, Push, NotificationHub)
builder.Services.AddInfrastructure(builder.Configuration);

// MassTransit - Consume all domain events
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderEventConsumer>();
    x.AddConsumer<PaymentEventConsumer>();
    x.AddConsumer<InventoryEventConsumer>();
    x.AddConsumer<VendorEventConsumer>();
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<SendEmailNotificationConsumer>();
    x.AddConsumer<SendPushNotificationConsumer>();
    x.AddConsumer<SendSmsNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost", h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest");
            h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseMetricServer();
app.UseHttpMetrics();
app.UseHangfireDashboard();
app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notification");

app.MapServiceDefaultsHealthChecks();
app.MapGet("/", () => Results.Ok(new { Service = "CommerceHub Notification Service", Status = "Running" }));
await app.RunAsync();
