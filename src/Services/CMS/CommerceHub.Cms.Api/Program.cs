using System.Text.RegularExpressions;
using CommerceHub.Cms.Application;
using CommerceHub.Cms.Infrastructure;
using CommerceHub.Cms.Infrastructure.Data;
using CommerceHub.ServiceDefaults;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
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

builder.ConfigureServiceDefaults("CommerceHub-CMS");
builder.ConfigureHangfire("commercehub-cms");
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddCmsInfrastructure(builder.Configuration);

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

var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConn; options.InstanceName = "CommerceHub_CMS_"; });

builder.Services.AddHealthChecks();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
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
app.MapGet("/", () => Results.Ok(new { Service = "CommerceHub CMS Service", Status = "Running" }));
await app.RunAsync();
