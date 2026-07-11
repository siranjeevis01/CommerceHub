using System.Reflection;
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

// ========== SERILOG (console + optional file) ==========
var logDir = Environment.GetEnvironmentVariable("LOG_DIR") ?? "logs";
var logConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CommerceHub")
    .WriteTo.Console();

var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
if (!string.IsNullOrWhiteSpace(seqUrl))
    logConfig.WriteTo.Seq(seqUrl);

var logFileEnabled = Environment.GetEnvironmentVariable("LOG_FILE_ENABLED") ?? "true";
if (bool.TryParse(logFileEnabled, out var logFile) && logFile)
    logConfig.WriteTo.File(Path.Combine(logDir, "commercehub-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7);

Log.Logger = logConfig.CreateLogger();
builder.Host.UseSerilog();

// ========== ENVIRONMENT VARIABLE EXPANSION ==========
var configKeys = builder.Configuration.AsEnumerable().Select(kv => kv.Key).ToList();
foreach (var key in configKeys)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value) && value.Contains("${"))
        builder.Configuration[key] = Regex.Replace(value, @"\${([^}]+)}", m =>
            Environment.GetEnvironmentVariable(m.Groups[1].Value) ?? "");
}

// ========== SERVICE DEFAULTS (OpenTelemetry, Prometheus) ==========
builder.ConfigureServiceDefaults("CommerceHub");

// ========== JWT AUTHENTICATION ==========
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
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ========== CORS ==========
var allowedOrigins = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
    ?? "http://localhost:4200,http://localhost:8100,http://localhost:3000").Split(',', StringSplitOptions.RemoveEmptyEntries);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// ========== FORWARDED HEADERS ==========
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// ========== CONFIGURABLE RATE LIMITING ==========
var rateLimitPermit = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_PERMIT"), out var rlp) ? rlp : 100;
var rateLimitWindow = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_WINDOW_SEC"), out var rlw) ? rlw : 60;
var rateLimitQueue = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_QUEUE"), out var rlq) ? rlq : 2;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("Default", config =>
    {
        config.PermitLimit = rateLimitPermit;
        config.Window = TimeSpan.FromSeconds(rateLimitWindow);
        config.QueueLimit = rateLimitQueue;
    });
});

// ========== DYNAMIC CONTROLLER DISCOVERY ==========
var apiAssemblies = new[]
{
    typeof(CommerceHub.Identity.Api.Controllers.AuthController).Assembly,
    typeof(CommerceHub.Product.Api.Controllers.ProductController).Assembly,
    typeof(CommerceHub.Order.Api.Controllers.OrderController).Assembly,
    typeof(CommerceHub.Cart.Api.Controllers.CartController).Assembly,
    typeof(CommerceHub.Payment.Api.Controllers.PaymentController).Assembly,
    typeof(CommerceHub.Vendor.Api.Controllers.VendorController).Assembly,
    typeof(CommerceHub.Inventory.Api.Controllers.InventoryController).Assembly,
    typeof(CommerceHub.Notification.Api.Controllers.NotificationController).Assembly,
    typeof(CommerceHub.Cms.Api.Controllers.CouponController).Assembly,
    typeof(CommerceHub.Analytics.Api.Controllers.AnalyticsController).Assembly,
    typeof(CommerceHub.AIAgent.Api.Controllers.ChatController).Assembly,
};

builder.Services.AddControllers()
    .ConfigureApplicationPartManager(manager =>
    {
        foreach (var assembly in apiAssemblies)
            manager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(assembly));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== API VERSIONING ==========
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

var signalRBuilder = builder.Services.AddSignalR().AddJsonProtocol();

if (!string.IsNullOrWhiteSpace(redisConn))
{
    Log.Information("Redis configured — enabling distributed cache + SignalR backplane + Cart");
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
    Log.Information("Redis not configured — Cart uses in-memory storage, SignalR in-process");
    builder.Services.AddCartInfrastructure();
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPaymentInfrastructure(builder.Configuration);
builder.Services.AddVendorInfrastructure(builder.Configuration);
builder.Services.AddInventoryInfrastructure(builder.Configuration);
builder.Services.AddCmsInfrastructure(builder.Configuration);
builder.Services.AddAnalyticsInfrastructure(builder.Configuration);
builder.Services.AddAIAgentInfrastructure(builder.Configuration);

// ========== MASS TRANSIT (optional) ==========
var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
if (!string.IsNullOrWhiteSpace(rabbitHost))
{
    Log.Information("RabbitMQ configured — enabling MassTransit + saga state machine");
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
                        ?? throw new InvalidOperationException("Order database connection string missing for saga state");
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
            var rabbitVHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/";
            cfg.Host(rabbitHost, rabbitVHost, h =>
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

// ========== GRACEFUL DATABASE INITIALIZATION ==========
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var logger = sp.GetRequiredService<ILogger<Program>>();

    var dbContexts = new (string Name, DbContext? Context)[]
    {
        ("Identity", AppStartup.TryResolve<IdentityDbContext>(sp)),
        ("Product", AppStartup.TryResolve<ProductDbContext>(sp)),
        ("Order", AppStartup.TryResolve<OrderDbContext>(sp)),
        ("Payment", AppStartup.TryResolve<PaymentDbContext>(sp)),
        ("Vendor", AppStartup.TryResolve<VendorDbContext>(sp)),
        ("Inventory", AppStartup.TryResolve<InventoryDbContext>(sp)),
        ("Notification", AppStartup.TryResolve<NotificationDbContext>(sp)),
        ("CMS", AppStartup.TryResolve<CmsDbContext>(sp)),
        ("Analytics", AppStartup.TryResolve<AnalyticsDbContext>(sp)),
        ("AIAgent", AppStartup.TryResolve<AIAgentDbContext>(sp)),
    };

    foreach (var (name, ctx) in dbContexts)
    {
        if (ctx == null)
        {
            logger.LogWarning("{Name} DbContext not registered — skipping initialization", name);
            continue;
        }
        try
        {
            await ctx.Database.EnsureCreatedAsync();
            logger.LogInformation("{Name} database ready", name);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "{Name} database unavailable — app will start without it", name);
        }
    }
}

// ========== GLOBAL EXCEPTION HANDLER ==========
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    {
        // Client disconnected — no response needed
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "An unexpected error occurred",
                status = 500,
                instance = context.Request.Path
            }));
    }
});

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
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
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
    Redis = !string.IsNullOrWhiteSpace(redisConn) ? "enabled" : "disabled (in-memory)",
    RabbitMQ = !string.IsNullOrWhiteSpace(rabbitHost) ? "enabled" : "disabled",
    Timestamp = DateTime.UtcNow
})).AllowAnonymous();

Log.Information("CommerceHub API started — Redis: {Redis}, RabbitMQ: {RabbitMQ}",
    !string.IsNullOrWhiteSpace(redisConn) ? "enabled" : "disabled (in-memory)",
    !string.IsNullOrWhiteSpace(rabbitHost) ? "enabled" : "disabled");

await app.RunAsync();
Log.CloseAndFlush();

internal static partial class AppStartup
{
    internal static T? TryResolve<T>(IServiceProvider sp) where T : class
    {
        try { return sp.GetRequiredService<T>(); }
        catch { return null; }
    }
}
