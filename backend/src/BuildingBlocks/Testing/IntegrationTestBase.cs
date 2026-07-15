using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommerceHub.Shared.Testing;

public abstract class IntegrationTestBase<TStartup> : IAsyncLifetime
    where TStartup : class
{
    protected readonly WebApplicationFactory<TStartup> Factory;
    protected HttpClient Client { get; private set; } = null!;

    protected IntegrationTestBase()
    {
        Factory = new WebApplicationFactory<TStartup>()
            .WithWebHostBuilder(ConfigureWebHost);
    }

    protected virtual void ConfigureWebHost(IWebHostBuilder builder)
    {
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    public virtual Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        Client?.Dispose();
        await Factory.DisposeAsync();
    }
}
