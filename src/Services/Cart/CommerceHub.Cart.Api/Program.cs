using System.Text.RegularExpressions;
using CommerceHub.Cart.Application;
using CommerceHub.Cart.Infrastructure;
using CommerceHub.ServiceDefaults;
using MassTransit;
using Prometheus;
using Serilog;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

var configKeys = builder.Configuration.AsEnumerable().Select(kv => kv.Key).ToList();
foreach (var key in configKeys)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value) && value.Contains("${"))
        builder.Configuration[key] = Regex.Replace(value, @"\${([^}]+)}", m => Environment.GetEnvironmentVariable(m.Groups[1].Value) ?? "");
}

builder.ConfigureServiceDefaults("CommerceHub-Cart");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "localhost:6379";
builder.Services.AddApplication();
builder.Services.AddCartInfrastructure(redisConn);

builder.Services.AddMassTransit(x =>
{
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
app.MapControllers();
app.MapServiceDefaultsHealthChecks();
app.MapGet("/", () => Results.Ok(new { Service = "CommerceHub Cart Service", Status = "Running" }));
await app.RunAsync();
