using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Ai.Application.Common.Interfaces;
using CommerceHub.Modules.Ai.Infrastructure.Data;
using CommerceHub.Modules.Ai.Infrastructure.Services;

namespace CommerceHub.Modules.Ai.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAIAgentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Ai")
            ?? Environment.GetEnvironmentVariable("AI_DB_CONNECTION")
            ?? throw new InvalidOperationException("AI Agent database connection string missing");

        services.AddDbContext<AIAgentDbContext>(options =>
            options.UseMySQL(connectionString, mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));

        services.AddScoped<IAIAgentDbContext>(provider => provider.GetRequiredService<AIAgentDbContext>());

        services.AddHttpClient<ILLMService, LLMService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IProductSearchService, ProductSearchService>(client =>
        {
            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("PRODUCT_SERVICE_URL") ?? "http://commercehub-product:8080/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<IRecommendationService, RecommendationService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        var redisConnection = configuration.GetConnectionString("Redis")
            ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "CommerceHub_AI_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
