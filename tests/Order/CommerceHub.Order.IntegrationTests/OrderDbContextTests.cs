using CommerceHub.Order.Domain.Entities;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommerceHub.Order.IntegrationTests;

public class OrderDbContextTests : IntegrationTestBase
{
    [Fact]
    public async Task OrderDbContext_ShouldCreateAndRetrieveOrder()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new OrderDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var order = new global::CommerceHub.Order.Domain.Entities.Order
        {
            UserId = 1,
            OrderNumber = "ORD-INT-001",
            OrderStatus = "Pending",
            PaymentStatus = "Pending",
            TotalAmount = 150.00m,
            Subtotal = 150.00m,
            PaymentMethod = "Credit Card",
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var retrieved = await context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == "ORD-INT-001");
        retrieved.Should().NotBeNull();
        retrieved!.TotalAmount.Should().Be(150.00m);
        retrieved.UserId.Should().Be(1);
    }

    [Fact]
    public async Task OrderDbContext_ShouldHandleOrderWithItems()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new OrderDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var order = new global::CommerceHub.Order.Domain.Entities.Order
        {
            UserId = 1,
            OrderNumber = "ORD-INT-002",
            OrderStatus = "Confirmed",
            PaymentStatus = "Pending",
            TotalAmount = 300.00m,
            Subtotal = 300.00m,
            PaymentMethod = "Stripe",
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 100.00m, TotalPrice = 200.00m },
                new() { ProductId = 2, Quantity = 1, UnitPrice = 100.00m, TotalPrice = 100.00m }
            }
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var retrieved = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == "ORD-INT-002");

        retrieved.Should().NotBeNull();
        retrieved!.Items.Should().HaveCount(2);
        retrieved.Items.Sum(i => i.TotalPrice).Should().Be(300.00m);
    }
}
