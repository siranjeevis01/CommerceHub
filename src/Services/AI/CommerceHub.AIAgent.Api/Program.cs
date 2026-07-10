using System.Text.RegularExpressions;
using Asp.Versioning;
using CommerceHub.AIAgent.Api.Hubs;
using CommerceHub.AIAgent.Application;
using CommerceHub.AIAgent.Infrastructure;
using CommerceHub.AIAgent.Infrastructure.Data;
using CommerceHub.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;
using System.Text;

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

builder.ConfigureServiceDefaults("CommerceHub-AIAgent");

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
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
}

builder.Services.AddAuthorization();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddApplication();
builder.Services.AddAIAgentInfrastructure(builder.Configuration);

builder.Services.AddSignalR().AddJsonProtocol();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AIAgentDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();
app.MapHub<AIHub>("/hubs/ai");
app.MapServiceDefaultsHealthChecks();

app.MapGet("/", () => Results.Ok(new { Service = "CommerceHub AI Agent Service", Status = "Running" }));

await app.RunAsync();