using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Infrastructure.Data;
using CommerceHub.Modules.Identity.Infrastructure.Repositories;
using CommerceHub.Modules.Identity.Infrastructure.Services;
using CommerceHub.Modules.Identity.Infrastructure.BackgroundJobs;

namespace CommerceHub.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Identity")
            ?? Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION")
            ?? throw new InvalidOperationException("Identity database connection string missing");
        if (!connectionString.Contains("SslMode")) connectionString += ";SslMode=Required;AllowPublicKeyRetrieval=true";

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseMySQL(connectionString,
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));

        services.AddScoped<IIdentityDbContext>(provider => provider.GetRequiredService<IdentityDbContext>());
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IBruteForceProtectionService, BruteForceProtectionService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();

        services.AddHostedService<OtpCleanupService>();

        return services;
    }
}
