using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Inventory.Infrastructure.Data;
using CommerceHub.Inventory.Application.Common.Interfaces;

namespace CommerceHub.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("INVENTORY_DB_CONNECTION")
            ?? throw new InvalidOperationException("Inventory database connection string missing");
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseMySQL(connStr, mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));
        services.AddScoped<IInventoryDbContext>(sp => sp.GetRequiredService<InventoryDbContext>());
        return services;
    }
}
