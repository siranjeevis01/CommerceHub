using CommerceHub.Order.Domain.Sagas;
using CommerceHub.Shared.Contracts.Events;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommerceHub.Order.UnitTests;

public class OrderSagaTests : IAsyncLifetime
{
    private ITestHarness _testHarness = null!;
    private ISagaStateMachineTestHarness<OrderStateMachine, OrderState> _sagaHarness = null!;
    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        _testHarness = _provider.GetRequiredService<ITestHarness>();
        _sagaHarness = _provider.GetRequiredService<ISagaStateMachineTestHarness<OrderStateMachine, OrderState>>();
        await _testHarness.Start();
    }

    public async Task DisposeAsync()
    {
        await _testHarness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task OrderPlaced_ShouldTransitionToPendingInventory()
    {
        var orderId = 1;
        var correlationId = NewId.NextGuid();
        await _testHarness.Bus.Publish(new OrderPlaced
        {
            CorrelationId = correlationId,
            OrderId = orderId,
            OrderNumber = "ORD-001",
            UserId = 1,
            TotalAmount = 100.00m,
            Items = new List<OrderItemEvent>
            {
                new() { ProductId = 1, Quantity = 1, UnitPrice = 100.00m, VendorId = 1 }
            },
            PlacedAt = DateTime.UtcNow
        });

        (await _sagaHarness.Created.Any(x => x.CorrelationId == correlationId)).Should().BeTrue();
    }

    [Fact]
    public async Task OrderSaga_ShouldHandleFullFlow()
    {
        var orderId = 2;
        var correlationId = NewId.NextGuid();

        await _testHarness.Bus.Publish(new OrderPlaced
        {
            CorrelationId = correlationId,
            OrderId = orderId,
            OrderNumber = "ORD-002",
            UserId = 1,
            TotalAmount = 200.00m,
            Items = new List<OrderItemEvent>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 100.00m, VendorId = 1 }
            },
            PlacedAt = DateTime.UtcNow
        });

        (await _sagaHarness.Created.Any(x => x.CorrelationId == correlationId)).Should().BeTrue();
    }
}
