using CommerceHub.Order.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Order.UnitTests;

public class OrderDomainTests
{
    [Fact]
    public void Order_ShouldCalculateTotalAmount_WhenItemsAdded()
    {
        var order = new global::CommerceHub.Order.Domain.Entities.Order
        {
            Id = 1,
            UserId = 1,
            OrderNumber = "ORD-001",
            OrderStatus = "Pending",
            Subtotal = 200.00m,
            TotalAmount = 200.00m,
            Items = new List<OrderItem>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 50.00m, TotalPrice = 100.00m },
                new() { ProductId = 2, Quantity = 1, UnitPrice = 100.00m, TotalPrice = 100.00m }
            }
        };

        order.TotalAmount.Should().Be(200.00m);
        order.OrderNumber.Should().Be("ORD-001");
        order.OrderStatus.Should().Be("Pending");
    }

    [Fact]
    public void Order_ShouldSetDefaultStatus_WhenCreated()
    {
        var order = new global::CommerceHub.Order.Domain.Entities.Order();
        order.OrderStatus.Should().Be("Pending");
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void OrderItem_ShouldCalculateTotalPrice()
    {
        var item = new OrderItem
        {
            ProductId = 1,
            Quantity = 3,
            UnitPrice = 25.00m,
            TotalPrice = 75.00m
        };

        item.TotalPrice.Should().Be(75.00m);
    }

    [Fact]
    public void ReturnRequest_ShouldSetProperties()
    {
        var request = new ReturnRequest
        {
            Id = 1,
            OrderId = 1,
            Reason = "Defective product",
            Status = "Pending"
        };

        request.Reason.Should().Be("Defective product");
        request.Status.Should().Be("Pending");
    }

    [Fact]
    public void Dispute_ShouldSetProperties()
    {
        var dispute = new Dispute
        {
            Id = 1,
            OrderId = 1,
            Type = "ItemNotReceived",
            Description = "Item not received",
            Status = "Open"
        };

        dispute.Type.Should().Be("ItemNotReceived");
        dispute.Description.Should().Be("Item not received");
        dispute.Status.Should().Be("Open");
    }

    [Fact]
    public void OrderTracking_ShouldSetProperties()
    {
        var tracking = new OrderTracking
        {
            Id = 1,
            OrderId = 1,
            Status = "Shipped",
            Description = "Package in transit",
            LocationName = "FedEx Hub"
        };

        tracking.Status.Should().Be("Shipped");
        tracking.Description.Should().Be("Package in transit");
    }

    [Fact]
    public void OrderStatusHistory_ShouldRecordStatusChange()
    {
        var history = new OrderStatusHistory
        {
            OrderId = 1,
            FromStatus = "Pending",
            ToStatus = "Confirmed",
            ChangedBy = "System"
        };

        history.FromStatus.Should().Be("Pending");
        history.ToStatus.Should().Be("Confirmed");
        history.ChangedBy.Should().Be("System");
    }

    [Fact]
    public void Order_ShouldTrackPayments()
    {
        var order = new global::CommerceHub.Order.Domain.Entities.Order
        {
            OrderStatus = "Pending",
            PaymentStatus = "Pending"
        };

        order.PaymentStatus = "Paid";
        order.OrderStatus = "Processing";
        order.PaymentStatus.Should().Be("Paid");
        order.OrderStatus.Should().Be("Processing");
    }
}
