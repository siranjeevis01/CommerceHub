using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Analytics.Infrastructure.Data;

namespace CommerceHub.Modules.Analytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("Analytics")
            ?? Environment.GetEnvironmentVariable("ANALYTICS_DB_CONNECTION")
            ?? throw new InvalidOperationException("Analytics database connection string missing");

        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(connStr, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(5)));

        return services;
    }
}
