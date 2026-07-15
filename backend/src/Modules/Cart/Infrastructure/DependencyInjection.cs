using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using CommerceHub.Modules.Cart.Application.Common.Interfaces;
using CommerceHub.Modules.Cart.Infrastructure.Repositories;

namespace CommerceHub.Modules.Cart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCartInfrastructure(this IServiceCollection services, string redisConnectionString)
    {
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<ICartRepository, CartRepository>();
        return services;
    }

    public static IServiceCollection AddCartInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICartRepository, InMemoryCartRepository>();
        return services;
    }
}
