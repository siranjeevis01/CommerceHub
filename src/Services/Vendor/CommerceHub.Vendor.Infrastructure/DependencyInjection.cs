using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Vendor.Infrastructure.Data;
using CommerceHub.Vendor.Application.Common.Interfaces;

namespace CommerceHub.Vendor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVendorInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("VENDOR_DB_CONNECTION")
            ?? throw new InvalidOperationException("Vendor database connection string missing");
        services.AddDbContext<VendorDbContext>(options =>
            options.UseMySQL(connStr, mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));
        services.AddScoped<IVendorDbContext>(sp => sp.GetRequiredService<VendorDbContext>());
        return services;
    }
}
