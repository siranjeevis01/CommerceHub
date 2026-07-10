using Testcontainers.MySql;
using Testcontainers.Redis;
using Testcontainers.RabbitMq;
using Xunit;

namespace CommerceHub.TestBase;

public class IntegrationTestBase : IAsyncLifetime
{
    protected MySqlContainer? MySqlContainer { get; private set; }
    protected RedisContainer? RedisContainer { get; private set; }
    protected RabbitMqContainer? RabbitMqContainer { get; private set; }
    protected string MySqlConnectionString => MySqlContainer?.GetConnectionString() ?? "";
    protected string RedisConnectionString => RedisContainer?.GetConnectionString() ?? "";
    protected string RabbitMqConnectionString => RabbitMqContainer?.GetConnectionString() ?? "";

    public virtual async Task InitializeAsync()
    {
        MySqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("testpassword")
            .Build();
        await MySqlContainer.StartAsync();

        RedisContainer = new RedisBuilder()
            .WithImage("redis:7.2-alpine")
            .Build();
        await RedisContainer.StartAsync();

        RabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.12-management-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
        await RabbitMqContainer.StartAsync();
    }

    public virtual async Task DisposeAsync()
    {
        if (MySqlContainer != null) await MySqlContainer.DisposeAsync();
        if (RedisContainer != null) await RedisContainer.DisposeAsync();
        if (RabbitMqContainer != null) await RabbitMqContainer.DisposeAsync();
    }
}
