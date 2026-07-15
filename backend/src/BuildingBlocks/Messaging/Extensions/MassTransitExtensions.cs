using System.Data.Common;
using System.Net.Sockets;
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
                    retry.Handle<DbException>();
                    retry.Handle<SocketException>();
                    retry.Handle<TimeoutException>();
                    retry.Ignore<ArgumentException>();
                    retry.Ignore<InvalidOperationException>();
                    retry.Exponential(3,
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(5));
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
