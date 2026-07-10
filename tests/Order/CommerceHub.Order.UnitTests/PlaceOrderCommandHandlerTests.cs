using CommerceHub.Order.Application.Commands;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Domain.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.Shared.Contracts.Events;
using CommerceHub.TestBase;

namespace CommerceHub.Order.UnitTests;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderDbContext> _contextMock;
    private readonly Mock<IOrderNumberGenerator> _orderNumberGeneratorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _contextMock = new Mock<IOrderDbContext>();
        _orderNumberGeneratorMock = new Mock<IOrderNumberGenerator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new PlaceOrderCommandHandler(
            _contextMock.Object,
            _orderNumberGeneratorMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WithGeneratedOrderNumber()
    {
        var orders = new List<global::CommerceHub.Order.Domain.Entities.Order>().AsQueryable();
        var mockOrders = orders.BuildMockDbSet();
        _contextMock.Setup(x => x.Orders).Returns(mockOrders.Object);

        _orderNumberGeneratorMock.Setup(x => x.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ORD-TEST-001");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _contextMock.Setup(x => x.Orders.Add(It.IsAny<global::CommerceHub.Order.Domain.Entities.Order>()))
            .Callback<global::CommerceHub.Order.Domain.Entities.Order>(o => { o.Id = 1; });

        var command = new PlaceOrderCommand
        {
            UserId = 1,
            SubTotal = 100.00m,
            DiscountAmount = 10.00m,
            ShippingCost = 5.00m,
            TaxAmount = 8.00m,
            ShippingAddress = "123 Main St",
            Items = new List<PlaceOrderItemDto>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 50.00m,
                    VendorId = 1
                }
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeGreaterThan(0);
        _contextMock.Verify(x => x.Orders.Add(It.IsAny<global::CommerceHub.Order.Domain.Entities.Order>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<OrderPlaced>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalAmount_Correctly()
    {
        var orders = new List<global::CommerceHub.Order.Domain.Entities.Order>().AsQueryable();
        var mockOrders = orders.BuildMockDbSet();
        _contextMock.Setup(x => x.Orders).Returns(mockOrders.Object);

        _orderNumberGeneratorMock.Setup(x => x.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ORD-002");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        global::CommerceHub.Order.Domain.Entities.Order? capturedOrder = null;
        _contextMock.Setup(x => x.Orders.Add(It.IsAny<global::CommerceHub.Order.Domain.Entities.Order>()))
            .Callback<global::CommerceHub.Order.Domain.Entities.Order>(o => capturedOrder = o);

        var command = new PlaceOrderCommand
        {
            UserId = 1,
            SubTotal = 200.00m,
            DiscountAmount = 20.00m,
            ShippingCost = 10.00m,
            TaxAmount = 15.00m,
            ShippingAddress = "456 Oak Ave",
            Items = new List<PlaceOrderItemDto>
            {
                new() { ProductId = 1, Quantity = 1, UnitPrice = 200.00m, VendorId = 1 }
            }
        };

        await _handler.Handle(command, CancellationToken.None);

        capturedOrder.Should().NotBeNull();
        capturedOrder!.OrderNumber.Should().Be("ORD-002");
        capturedOrder.Subtotal.Should().Be(200.00m);
        capturedOrder.DiscountAmount.Should().Be(20.00m);
        capturedOrder.ShippingCost.Should().Be(10.00m);
        capturedOrder.TaxAmount.Should().Be(15.00m);
        capturedOrder.TotalAmount.Should().Be(205.00m);
        capturedOrder.OrderStatus.Should().Be("Pending");
        capturedOrder.PaymentStatus.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_ShouldAddStatusHistory_WhenOrderCreated()
    {
        var orders = new List<global::CommerceHub.Order.Domain.Entities.Order>().AsQueryable();
        var mockOrders = orders.BuildMockDbSet();
        _contextMock.Setup(x => x.Orders).Returns(mockOrders.Object);

        _orderNumberGeneratorMock.Setup(x => x.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ORD-003");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        global::CommerceHub.Order.Domain.Entities.Order? capturedOrder = null;
        _contextMock.Setup(x => x.Orders.Add(It.IsAny<global::CommerceHub.Order.Domain.Entities.Order>()))
            .Callback<global::CommerceHub.Order.Domain.Entities.Order>(o => capturedOrder = o);

        var command = new PlaceOrderCommand
        {
            UserId = 1,
            SubTotal = 50.00m,
            TaxAmount = 5.00m,
            ShippingAddress = "789 Pine Rd",
            Items = new List<PlaceOrderItemDto>
            {
                new() { ProductId = 1, Quantity = 1, UnitPrice = 50.00m, VendorId = 1 }
            }
        };

        await _handler.Handle(command, CancellationToken.None);

        capturedOrder!.StatusHistories.Should().HaveCount(1);
        capturedOrder.StatusHistories.First().ToStatus.Should().Be("Pending");
        capturedOrder.StatusHistories.First().Remarks.Should().Be("Order placed");
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderItems()
    {
        var orders = new List<global::CommerceHub.Order.Domain.Entities.Order>().AsQueryable();
        var mockOrders = orders.BuildMockDbSet();
        _contextMock.Setup(x => x.Orders).Returns(mockOrders.Object);

        _orderNumberGeneratorMock.Setup(x => x.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ORD-004");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        global::CommerceHub.Order.Domain.Entities.Order? capturedOrder = null;
        _contextMock.Setup(x => x.Orders.Add(It.IsAny<global::CommerceHub.Order.Domain.Entities.Order>()))
            .Callback<global::CommerceHub.Order.Domain.Entities.Order>(o => capturedOrder = o);

        var command = new PlaceOrderCommand
        {
            UserId = 1,
            SubTotal = 300.00m,
            TaxAmount = 24.00m,
            ShippingAddress = "Addr",
            Items = new List<PlaceOrderItemDto>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 50.00m, VendorId = 1 },
                new() { ProductId = 2, Quantity = 1, UnitPrice = 100.00m, VendorId = 2 },
                new() { ProductId = 3, Quantity = 5, UnitPrice = 20.00m, VendorId = 1 }
            }
        };

        await _handler.Handle(command, CancellationToken.None);

        capturedOrder!.Items.Should().HaveCount(3);
        capturedOrder.Items.First().TotalPrice.Should().Be(100.00m);
        capturedOrder.Items.Last().TotalPrice.Should().Be(100.00m);
    }
}
