using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Modules.Product.Infrastructure.Data;
using CommerceHub.Modules.Product.Infrastructure.Services;
using Elastic.Clients.Elasticsearch;

namespace CommerceHub.Modules.Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Product")
            ?? Environment.GetEnvironmentVariable("PRODUCT_DB_CONNECTION")
            ?? throw new InvalidOperationException("Product database connection string missing");

        services.AddDbContext<ProductDbContext>(options =>
            options.UseMySQL(connectionString,
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));

        services.AddScoped<IProductDbContext>(provider => provider.GetRequiredService<ProductDbContext>());

        var esUrl = configuration["Elasticsearch:Url"]
            ?? Environment.GetEnvironmentVariable("ELASTICSEARCH_URL")
            ?? "http://localhost:9200";
        var esSettings = new ElasticsearchClientSettings(new Uri(esUrl)).DefaultIndex("products");
        services.AddSingleton(new ElasticsearchClient(esSettings));
        services.AddScoped<IProductSearchService, ProductSearchService>();

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
