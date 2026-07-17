using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Modules.Product.Infrastructure.Data;
using CommerceHub.Modules.Product.Infrastructure.Services;

namespace CommerceHub.Modules.Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Product")
            ?? Environment.GetEnvironmentVariable("PRODUCT_DB_CONNECTION")
            ?? throw new InvalidOperationException("Product database connection string missing");
        if (!connectionString.Contains("SslMode")) connectionString += ";SslMode=Required;AllowPublicKeyRetrieval=true";

        services.AddDbContext<ProductDbContext>(options =>
            options.UseMySQL(connectionString,
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));

        services.AddScoped<IProductDbContext>(provider => provider.GetRequiredService<ProductDbContext>());

        var searchUrl = configuration["Meilisearch:Url"]
            ?? Environment.GetEnvironmentVariable("MEILISEARCH_URL");
        var searchApiKey = configuration["Meilisearch:ApiKey"]
            ?? Environment.GetEnvironmentVariable("MEILISEARCH_API_KEY");

        if (!string.IsNullOrWhiteSpace(searchUrl) && !searchUrl.Contains("${"))
        {
            var msClient = string.IsNullOrWhiteSpace(searchApiKey) || searchApiKey.Contains("${")
                ? new Meilisearch.MeilisearchClient(searchUrl)
                : new Meilisearch.MeilisearchClient(searchUrl, searchApiKey);
            services.AddSingleton(msClient);
        }

        services.AddScoped<IProductSearchService>(sp =>
        {
            var db = sp.GetRequiredService<ProductDbContext>();
            var logger = sp.GetRequiredService<ILogger<ProductSearchService>>();
            var client = sp.GetService<Meilisearch.MeilisearchClient>();
            return new ProductSearchService(db, logger, client);
        });

        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "CommerceHub_Product_";
            });
        }

        return services;
    }
}
