using System.IO.Compression;
using System.Text;
using Asp.Versioning;
using CommerceHub.Api.Middleware;
using CommerceHub.Infrastructure.Configurations;
using CommerceHub.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;
using CommerceHub.Modules.Identity.Infrastructure;
using CommerceHub.Modules.Product.Infrastructure;
using CommerceHub.Modules.Order.Infrastructure;
using CommerceHub.Modules.Order.Infrastructure.Data;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;
using CommerceHub.Modules.Notification.Infrastructure;
using CommerceHub.Modules.Cart.Infrastructure;
using CommerceHub.Modules.Payment.Infrastructure;
using CommerceHub.Modules.Vendor.Infrastructure;
using CommerceHub.Modules.Inventory.Infrastructure;
using CommerceHub.Modules.Cms.Infrastructure;
using CommerceHub.Modules.Analytics.Infrastructure;
using CommerceHub.Modules.Ai.Infrastructure;
using CommerceHub.Shared.Messaging.Extensions;
using CommerceHub.Infrastructure.Persistence;
using CommerceHub.Api.Seeder;
using CommerceHub.Modules.Identity.Infrastructure.Data;
using CommerceHub.Modules.Product.Infrastructure.Data;
using CommerceHub.Modules.Payment.Infrastructure.Data;
using CommerceHub.Modules.Vendor.Infrastructure.Data;
using CommerceHub.Modules.Inventory.Infrastructure.Data;
using CommerceHub.Modules.Notification.Infrastructure.Persistence;
using CommerceHub.Modules.Cms.Infrastructure.Data;
using CommerceHub.Modules.Analytics.Infrastructure.Data;
using CommerceHub.Modules.Ai.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Prometheus;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// ========== ENVIRONMENT VARIABLE EXPANSION ==========
var configKeys = builder.Configuration.AsEnumerable().Select(kv => kv.Key).ToList();
foreach (var key in configKeys)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value) && value.Contains("${"))
        builder.Configuration[key] = System.Text.RegularExpressions.Regex.Replace(value, @"\${([^}]+)}", m =>
            Environment.GetEnvironmentVariable(m.Groups[1].Value) ?? "");
}

// ========== CONFIGURATION OPTIONS ==========
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection(RateLimitOptions.SectionName));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<SeqOptions>(builder.Configuration.GetSection(SeqOptions.SectionName));
builder.Services.Configure<OtlpOptions>(builder.Configuration.GetSection(OtlpOptions.SectionName));
builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection(LoggingOptions.SectionName));
builder.Services.Configure<SmtpOptions>(options =>
{
    var section = builder.Configuration.GetSection(SmtpOptions.SectionName);
    var server = section["Server"];
    if (string.IsNullOrEmpty(server) || server.StartsWith("${"))
        server = Environment.GetEnvironmentVariable("SMTP_HOST");
    if (!string.IsNullOrEmpty(server)) options.Server = server;

    var portStr = section["Port"];
    if (string.IsNullOrEmpty(portStr) || portStr.StartsWith("${"))
        portStr = Environment.GetEnvironmentVariable("SMTP_PORT");
    if (int.TryParse(portStr, out var port)) options.Port = port;

    var email = section["SenderEmail"];
    if (string.IsNullOrEmpty(email) || email.StartsWith("${"))
        email = Environment.GetEnvironmentVariable("SMTP_USER") ?? Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");
    if (!string.IsNullOrEmpty(email)) options.SenderEmail = email;

    var pass = section["SenderPassword"];
    if (string.IsNullOrEmpty(pass) || pass.StartsWith("${"))
        pass = Environment.GetEnvironmentVariable("SMTP_PASS");
    if (!string.IsNullOrEmpty(pass)) options.SenderPassword = pass;

    var sslStr = section["EnableSsl"];
    if (string.IsNullOrEmpty(sslStr) || sslStr.StartsWith("${"))
        sslStr = Environment.GetEnvironmentVariable("Smtp__EnableSsl");
    if (bool.TryParse(sslStr, out var ssl)) options.EnableSsl = ssl;
});
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));
builder.Services.Configure<RazorpayOptions>(builder.Configuration.GetSection(RazorpayOptions.SectionName));
builder.Services.Configure<TwilioOptions>(builder.Configuration.GetSection(TwilioOptions.SectionName));
builder.Services.Configure<WhatsAppOptions>(builder.Configuration.GetSection(WhatsAppOptions.SectionName));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
builder.Services.Configure<HangfireOptions>(builder.Configuration.GetSection(HangfireOptions.SectionName));
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.SectionName));

// ========== SERILOG ==========
var logConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CommerceHub")
    .WriteTo.Console();

var seqSection = builder.Configuration.GetSection(SeqOptions.SectionName);
var seqUrl = seqSection["Url"];
if (!string.IsNullOrWhiteSpace(seqUrl))
    logConfig.WriteTo.Seq(seqUrl);

Log.Logger = logConfig.CreateLogger();
builder.Host.UseSerilog();

// ========== SERVICE DEFAULTS ==========
builder.ConfigureServiceDefaults("CommerceHub");

// ========== JWT AUTHENTICATION ==========
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtKey = jwtSection["Key"] ?? "";
if (string.IsNullOrEmpty(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("JWT Key must be configured and be at least 32 bytes.");

var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "CommerceHub",
            ValidAudience = jwtSection["Audience"] ?? "CommerceHubClient",
            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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
var corsSection = builder.Configuration.GetSection(CorsOptions.SectionName);
var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200", "http://localhost:8100", "http://localhost:3000"];
var envOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
if (!string.IsNullOrWhiteSpace(envOrigins))
{
    var envList = envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    allowedOrigins = [.. allowedOrigins, .. envList];
}
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// ========== FORWARDED HEADERS ==========
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto;
});

// ========== SIGNALR ==========
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ========== RATE LIMITING ==========
var rlSection = builder.Configuration.GetSection(RateLimitOptions.SectionName);
var rateLimitPermit = rlSection.GetValue<int>("PermitLimit", 100);
var rateLimitWindow = rlSection.GetValue<int>("WindowSeconds", 60);
var rateLimitQueue = rlSection.GetValue<int>("QueueLimit", 2);

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

// ========== RESPONSE COMPRESSION ==========
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// ========== OUTPUT CACHING ==========
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("Products", b => b.Expire(TimeSpan.FromMinutes(2)).SetVaryByQuery("page", "pageSize", "categoryId", "search"));
    options.AddPolicy("Cms", b => b.Expire(TimeSpan.FromMinutes(10)));
    options.AddPolicy("Health", b => b.Expire(TimeSpan.FromSeconds(30)));
});

// ========== API VERSIONING + SWAGGER ==========
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CommerceHub.Modules.Identity.Presentation.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Product.Presentation.Controllers.ProductController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Order.Presentation.Controllers.OrderController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Cart.Presentation.Controllers.CartController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Payment.Presentation.Controllers.PaymentController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Vendor.Presentation.Controllers.VendorController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Inventory.Presentation.Controllers.InventoryController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Notification.Presentation.Controllers.NotificationController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Cms.Presentation.Controllers.MenuController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Analytics.Presentation.Controllers.AnalyticsController).Assembly)
    .AddApplicationPart(typeof(CommerceHub.Modules.Ai.Presentation.Controllers.ChatController).Assembly);
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CommerceHub API", Version = "v1", Description = "CommerceHub multi-vendor e-commerce modular monolith API" });
    options.CustomSchemaIds(t => t.FullName?.Replace("CommerceHub.", "").Replace("+", "."));
});

// ========== MODULE REGISTRATION ==========
// Identity Module
CommerceHub.Modules.Identity.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// Product Module
CommerceHub.Modules.Product.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddProductInfrastructure(builder.Configuration);

// Order Module
CommerceHub.Modules.Order.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddOrderInfrastructure(builder.Configuration);

// Cart Module
CommerceHub.Modules.Cart.Application.DependencyInjection.AddApplication(builder.Services);
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
    ?? builder.Configuration.GetSection("Redis")["ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnectionString) && !redisConnectionString.Contains("${"))
{
    builder.Services.AddCartInfrastructure(redisConnectionString);
    Log.Information("Cart module using Redis backend");
}
else
{
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddCartInfrastructure();
    Log.Warning("Cart module using InMemory backend (Redis not configured)");
}

// Payment Module
CommerceHub.Modules.Payment.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddPaymentInfrastructure(builder.Configuration);

// Vendor Module
CommerceHub.Modules.Vendor.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddVendorInfrastructure(builder.Configuration);

// Inventory Module
CommerceHub.Modules.Inventory.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddInventoryInfrastructure(builder.Configuration);

// Notification Module
CommerceHub.Modules.Notification.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddNotificationInfrastructure(builder.Configuration);

// Cms Module
CommerceHub.Modules.Cms.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddCmsInfrastructure(builder.Configuration);

// Analytics Module
CommerceHub.Modules.Analytics.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddAnalyticsInfrastructure(builder.Configuration);

// Ai Module
CommerceHub.Modules.Ai.Application.DependencyInjection.AddApplication(builder.Services);
builder.Services.AddAIAgentInfrastructure(builder.Configuration);

// ========== DB INITIALIZERS ==========
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<IdentityDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<ProductDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<OrderDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<PaymentDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<VendorDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<InventoryDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<NotificationDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<CmsDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<AnalyticsDbContext>>();
builder.Services.AddTransient<IDbInitializer, DatabaseInitializer<AIAgentDbContext>>();

// ========== DATABASE SEEDER ==========
builder.Services.AddScoped<DatabaseSeeder>();

// ========== MASSTRANSIT / RABBITMQ ==========
var rabbitMqHost = builder.Configuration.GetSection("RabbitMQ")["Host"]
    ?? Environment.GetEnvironmentVariable("RABBITMQ_HOST");
if (!string.IsNullOrWhiteSpace(rabbitMqHost))
{
    var rabbitMqUser = builder.Configuration.GetSection("RabbitMQ")["Username"]
        ?? Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
    var rabbitMqPass = builder.Configuration.GetSection("RabbitMQ")["Password"]
        ?? Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";
    var rabbitMqVhost = builder.Configuration.GetSection("RabbitMQ")["VirtualHost"]
        ?? Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/";
    if (!rabbitMqVhost.StartsWith("/"))
        rabbitMqVhost = "/" + rabbitMqVhost;
    var rabbitConnectionString = $"amqp://{rabbitMqUser}:{rabbitMqPass}@{rabbitMqHost}{rabbitMqVhost}";

    builder.Services.AddServiceBus<OrderDbContext>("order", rabbitConnectionString, consumers =>
    {
        consumers.AddSagaStateMachine<CommerceHub.Modules.Order.Domain.Sagas.OrderStateMachine, CommerceHub.Modules.Order.Domain.Sagas.OrderState>()
            .InMemoryRepository();
        consumers.AddConsumersFromAssemblyContaining<CommerceHub.Modules.Notification.Presentation.Consumers.OrderEventConsumer>();
        consumers.AddConsumersFromAssemblyContaining<CommerceHub.Modules.Notification.Application.Consumers.SendEmailNotificationConsumer>();
        consumers.AddConsumersFromAssemblyContaining<CommerceHub.Modules.Payment.Presentation.Consumers.ProcessPaymentConsumer>();
        consumers.AddConsumersFromAssemblyContaining<CommerceHub.Modules.Analytics.Application.Consumers.AnalyticsEventConsumer>();
    });
    Log.Information("MassTransit configured with RabbitMQ at {Host}", rabbitMqHost);
}
else
{
    Log.Warning("RabbitMQ not configured - inter-module messaging disabled");
}

// ========== HANGFIRE ==========
builder.ConfigureHangfire("CommerceHub");

var app = builder.Build();

// ========== DATABASE INITIALIZATION ==========
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var dbInitializers = scope.ServiceProvider.GetServices<IDbInitializer>();
    foreach (var dbInitializer in dbInitializers)
    {
        try
        {
            await dbInitializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "{Initializer} failed - will retry on next request", dbInitializer.GetType().Name);
        }
    }

    if (builder.Configuration.GetValue<bool>("Seed:Enabled", true))
    {
        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAllAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database seeding failed");
        }
    }
}

// ========== MIDDLEWARE PIPELINE ==========
app.UseGlobalExceptionHandler();
app.UseResponseCompression();
app.UseForwardedHeaders();
app.UseApiRouteRewrites();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000ms}";
    options.GetLevel = (httpContext, elapsed, ex) =>
        ex != null ? LogEventLevel.Error
        : httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
        : httpContext.Response.StatusCode >= 400 ? LogEventLevel.Warning
        : LogEventLevel.Information;
});
app.UseSwagger();
app.UseSwaggerUI();
app.UseMetricServer();
app.UseHttpMetrics();
app.UseSecurityHeaders();

// ========== ENDPOINTS ==========
app.MapControllers().CacheOutput("Products");
app.MapHub<NotificationHub>("/hubs/notification");
app.MapTrackingHub();
app.MapServiceDefaultsHealthChecks();
app.UseHangfireDashboard();
app.MapGet("/", () => Results.Ok(new
{
    Service = "CommerceHub API",
    Version = "1.0.0",
    Architecture = "Modular Monolith",
    Status = "Running",
    Timestamp = DateTime.UtcNow
})).AllowAnonymous().CacheOutput("Health");

Log.Information("CommerceHub Modular Monolith API started");

await app.RunAsync();
Log.CloseAndFlush();
