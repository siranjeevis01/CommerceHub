using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceHub.Shared.Messaging.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddServiceBus<TDbContext>(this IServiceCollection services, string serviceName, string connectionString, Action<IBusRegistrationConfigurator>? configureConsumers = null)
        where TDbContext : DbContext
    {
        services.AddMassTransit(busConfig =>
        {
            busConfig.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter($"commercehub.{serviceName}", false));

            busConfig.UsingRabbitMq((context, config) =>
            {
                config.Host(connectionString);
                config.UseMessageRetry(retry =>
                {
                    retry.Ignore<ArgumentException>();
                    retry.Ignore<InvalidOperationException>();
                    retry.Interval(3, TimeSpan.FromSeconds(5));
                });
                config.ConfigureEndpoints(context);
            });

            configureConsumers?.Invoke(busConfig);
        });

        return services;
    }

    public static IBusRegistrationConfigurator AddConsumer<T>(this IBusRegistrationConfigurator configurator)
        where T : class, IConsumer
    {
        configurator.AddConsumer(typeof(T));
        return configurator;
    }

    public static IBusRegistrationConfigurator AddConsumersFromAssemblyContaining<T>(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumers(typeof(T).Assembly);
        return configurator;
    }
}
