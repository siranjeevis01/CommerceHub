using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using CommerceHub.AIAgent.Api.Hubs;
using CommerceHub.AIAgent.Infrastructure.Data;
using CommerceHub.Analytics.Infrastructure.Data;
using CommerceHub.Cms.Infrastructure.Data;
using CommerceHub.Identity.Infrastructure.Data;
using CommerceHub.Inventory.Infrastructure.Data;
using CommerceHub.Notification.Infrastructure.Persistence;
using CommerceHub.Notification.Infrastructure.Hubs;
using CommerceHub.Order.Domain.Sagas;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.Payment.Infrastructure.Data;
using CommerceHub.Product.Infrastructure.Data;
using CommerceHub.ServiceDefaults;
using CommerceHub.Vendor.Infrastructure.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;
using StackExchange.Redis;

// Infrastructure DI (unique names, safe to import)
using CommerceHub.Cart.Infrastructure;
using CommerceHub.Identity.Infrastructure;
using CommerceHub.Product.Infrastructure;
using CommerceHub.Order.Infrastructure;
using CommerceHub.Payment.Infrastructure;
using CommerceHub.Vendor.Infrastructure;
using CommerceHub.Inventory.Infrastructure;
using CommerceHub.Notification.Infrastructure;
using CommerceHub.Cms.Infrastructure;
using CommerceHub.Analytics.Infrastructure;
using CommerceHub.AIAgent.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CommerceHub")
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
    .CreateLogger();
builder.Host.UseSerilog();

// --- Environment variable expansion for ${VAR} in config ---
var configKeys = builder.Configuration.AsEnumerable().Select(kv => kv.Key).ToList();
foreach (var key in configKeys)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value) && value.Contains("${"))
        builder.Configuration[key] = Regex.Replace(value, @"\${([^}]+)}", m =>
            Environment.GetEnvironmentVariable(m.Groups[1].Value) ?? "");
}

// --- ServiceDefaults (OpenTelemetry, Prometheus) ---
builder.ConfigureServiceDefaults("CommerceHub");

// --- JWT Authentication ---
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"] ?? "";
if (string.IsNullOrEmpty(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("JWT_KEY must be configured and be at least 32 bytes.");
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "CommerceHub",
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "CommerceHubClient",
            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// --- CORS ---
var allowedOrigins = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
    ?? "http://localhost:4200,http://localhost:8100,http://localhost:3000").Split(',');
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// --- Forwarded Headers ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("Default", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 2;
    });
});

// --- Controllers (discovered from all API assemblies) ---
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CommerceHub.Identity.Api.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Product.Api.Controllers.ProductController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Order.Api.Controllers.OrderController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Cart.Api.Controllers.CartController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Payment.Api.Controllers.PaymentController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Vendor.Api.Controllers.VendorController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Inventory.Api.Controllers.InventoryController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Notification.Api.Controllers.NotificationController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Cms.Api.Controllers.CouponController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Analytics.Api.Controllers.AnalyticsController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.AIAgent.Api.Controllers.ChatController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- API Versioning ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ========== APPLICATION LAYER DI ==========
// Each AddApplication() is in its own namespace; call via static class to avoid ambiguity
CommerceHub.Identity.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Product.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Order.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Cart.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Payment.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Vendor.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Inventory.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Notification.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Cms.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.Analytics.Application.DependencyInjection.AddApplication(builder.Services);
CommerceHub.AIAgent.Application.DependencyInjection.AddApplication(builder.Services);

// ========== INFRASTRUCTURE LAYER DI ==========
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddProductInfrastructure(builder.Configuration);
builder.Services.AddOrderInfrastructure(builder.Configuration);

var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "";

// SignalR always registered (Redis backplane only if Redis available)
var signalRBuilder = builder.Services.AddSignalR().AddJsonProtocol();

if (!string.IsNullOrWhiteSpace(redisConn))
{
    builder.Services.AddCartInfrastructure(redisConn);
    signalRBuilder.AddStackExchangeRedis(redisConn);
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "CommerceHub_";
    });
}
else
{
    // Cart falls back to in-memory repository when Redis is unavailable
    builder.Services.AddCartInfrastructure();
    builder.Services.AddDistributedMemoryCache();
}

// Notification Infrastructure always registered (no Redis dependency)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddPaymentInfrastructure(builder.Configuration);
builder.Services.AddVendorInfrastructure(builder.Configuration);
builder.Services.AddInventoryInfrastructure(builder.Configuration);

builder.Services.AddCmsInfrastructure(builder.Configuration);
builder.Services.AddAnalyticsInfrastructure(builder.Configuration);
builder.Services.AddAIAgentInfrastructure(builder.Configuration);

// ========== MASS TRANSIT (optional: only if RABBITMQ_HOST is set) ==========
var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
if (!string.IsNullOrWhiteSpace(rabbitHost))
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<CommerceHub.Payment.Api.Consumers.ProcessPaymentConsumer>();
        x.AddConsumer<CommerceHub.Analytics.Application.Consumers.AnalyticsEventConsumer>();
        x.AddConsumer<CommerceHub.Notification.Api.Consumers.OrderEventConsumer>();
        x.AddConsumer<CommerceHub.Notification.Api.Consumers.PaymentEventConsumer>();
        x.AddConsumer<CommerceHub.Notification.Api.Consumers.InventoryEventConsumer>();
        x.AddConsumer<CommerceHub.Notification.Api.Consumers.VendorEventConsumer>();
        x.AddConsumer<CommerceHub.Notification.Application.Consumers.SendEmailNotificationConsumer>();
        x.AddConsumer<CommerceHub.Notification.Application.Consumers.SendPushNotificationConsumer>();
        x.AddConsumer<CommerceHub.Notification.Application.Consumers.SendSmsNotificationConsumer>();

        x.AddSagaStateMachine<OrderStateMachine, OrderState>()
            .EntityFrameworkRepository(r =>
            {
                r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                r.AddDbContext<OrderStateDbContext, OrderStateDbContext>((provider, builder) =>
                {
                    var connectionString = Environment.GetEnvironmentVariable("ORDER_DB_CONNECTION")
                        ?? throw new InvalidOperationException("Order database connection string missing");
                    builder.UseMySQL(connectionString, mysqlOptions => mysqlOptions.EnableRetryOnFailure(5));
                });
            });

        x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
        {
            o.QueryDelay = TimeSpan.FromSeconds(10);
            o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
            o.UseBusOutbox();
        });

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitHost, h =>
            {
                h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest");
                h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest");
            });
            cfg.UseMessageRetry(r => r.Exponential(5,
                TimeSpan.FromMilliseconds(200), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
            cfg.ConfigureEndpoints(context);
        });
    });
}

// ========== HEALTH CHECKS ==========
builder.Services.AddHealthChecks();

// ========== BUILD ==========
var app = builder.Build();

// ========== DATABASE INITIALIZATION ==========
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    await sp.GetRequiredService<IdentityDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<ProductDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<OrderDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<PaymentDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<VendorDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<InventoryDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<NotificationDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<CmsDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<AnalyticsDbContext>().Database.EnsureCreatedAsync();
    await sp.GetRequiredService<AIAgentDbContext>().Database.EnsureCreatedAsync();
}

// ========== MIDDLEWARE ==========
app.UseForwardedHeaders();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();
app.UseMetricServer();
app.UseHttpMetrics();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    await next();
});

// ========== ENDPOINTS ==========
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<AIHub>("/hubs/ai");
app.MapServiceDefaultsHealthChecks();
app.MapGet("/", () => Results.Ok(new
{
    Service = "CommerceHub API",
    Status = "Running",
    Timestamp = DateTime.UtcNow
})).AllowAnonymous();

await app.RunAsync();
