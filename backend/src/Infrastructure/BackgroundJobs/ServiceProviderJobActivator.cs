using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceHub.Infrastructure.BackgroundJobs;

public class ServiceProviderJobActivator : JobActivator
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderJobActivator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override object ActivateJob(Type jobType)
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService(jobType);
    }
}
