using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Cms.Infrastructure.Data;

namespace CommerceHub.Modules.Cms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCmsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("Cms")
            ?? Environment.GetEnvironmentVariable("CMS_DB_CONNECTION")
            ?? throw new InvalidOperationException("CMS database connection string missing");
        services.AddDbContext<CmsDbContext>(options =>
            options.UseMySQL(connStr, mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));
        return services;
    }
}
