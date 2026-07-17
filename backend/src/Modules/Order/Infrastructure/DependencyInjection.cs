using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Order.Application.Common.Interfaces;
using CommerceHub.Modules.Order.Infrastructure.Data;
using CommerceHub.Modules.Order.Infrastructure.BackgroundJobs;

namespace CommerceHub.Modules.Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Order")
            ?? Environment.GetEnvironmentVariable("ORDER_DB_CONNECTION")
            ?? throw new InvalidOperationException("Order database connection string missing");
        if (!connectionString.Contains("SslMode")) connectionString += ";SslMode=Required;AllowPublicKeyRetrieval=true";

        services.AddDbContext<OrderDbContext>(options =>
            options.UseMySQL(connectionString,
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));

        services.AddDbContext<OrderStateDbContext>(options =>
            options.UseMySQL(connectionString,
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));

        services.AddScoped<IOrderDbContext>(sp => sp.GetRequiredService<OrderDbContext>());
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();

        services.AddHostedService<OrderTimeoutService>();

        return services;
    }
}

internal class OrderNumberGenerator : IOrderNumberGenerator
{
    public Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var timestamp = now.ToString("yyyyMMddHHmmss");
        var suffix = Random.Shared.Next(1000, 9999).ToString();
        return Task.FromResult($"ORD-{timestamp}-{suffix}");
    }
}
