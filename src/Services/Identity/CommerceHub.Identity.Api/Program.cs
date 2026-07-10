using System.Text;
using System.Text.RegularExpressions;
using Asp.Versioning;
using CommerceHub.Identity.Application;
using CommerceHub.Identity.Infrastructure;
using CommerceHub.Identity.Infrastructure.Data;
using CommerceHub.ServiceDefaults;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var configKeys = builder.Configuration.AsEnumerable().Select(kv => kv.Key).ToList();
foreach (var key in configKeys)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value) && value.Contains("${"))
    {
        builder.Configuration[key] = Regex.Replace(value, @"\${([^}]+)}", m =>
            Environment.GetEnvironmentVariable(m.Groups[1].Value) ?? "");
    }
}

builder.ConfigureServiceDefaults("CommerceHub-Identity");
builder.ConfigureHangfire("commercehub-identity");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"] ?? "";
if (!string.IsNullOrEmpty(jwtKey))
{
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
        });
}

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") ?? "http://localhost:4200").Split(',');
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);

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

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMetricServer();
app.UseHttpMetrics();
app.UseHangfireDashboard();
app.MapControllers();
app.MapServiceDefaultsHealthChecks();

app.MapGet("/", () => Results.Ok(new { Service = "CommerceHub Identity Service", Status = "Running" }));

await app.RunAsync();
