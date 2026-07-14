using Asp.Versioning;
using CommerceHub.Order.Application;
using CommerceHub.Order.Domain.Sagas;
using CommerceHub.Order.Infrastructure;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.ServiceDefaults;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var configKeys = builder.Configuration.AsEnumerable().Select(kv => kv.Key).ToList();
foreach (var key in configKeys)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value) && value.Contains("${"))
    {
        builder.Configuration[key] = System.Text.RegularExpressions.Regex.Replace(value, @"\${([^}]+)}", m =>
            Environment.GetEnvironmentVariable(m.Groups[1].Value) ?? "");
    }
}

builder.ConfigureServiceDefaults("CommerceHub-Order");
builder.ConfigureHangfire("commercehub-order");

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var allowedOrigins = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
    ?? "http://localhost:4200,http://localhost:8100,http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

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

builder.Services.AddApplication();
builder.Services.AddOrderInfrastructure(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.AddDbContext<OrderStateDbContext, OrderStateDbContext>((provider, builder) =>
            {
                var connectionString = Environment.GetEnvironmentVariable("ORDER_DB_CONNECTION")
                    ?? throw new InvalidOperationException("Order database connection string missing");
                builder.UseMySQL(connectionString,
                    mysqlOptions => mysqlOptions.EnableRetryOnFailure(5));
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
        cfg.Host(Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost", h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest");
            h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest");
        });
        cfg.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromMilliseconds(200), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseMetricServer();
app.UseHttpMetrics();
app.UseHangfireDashboard();
app.MapControllers();
app.MapServiceDefaultsHealthChecks();
app.MapGet("/", () => Results.Ok(new { Service = "CommerceHub Order Service", Status = "Running" }));
await app.RunAsync();
