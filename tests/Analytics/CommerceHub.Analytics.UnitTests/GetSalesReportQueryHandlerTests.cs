using CommerceHub.Analytics.Application.Common.Interfaces;
using CommerceHub.Analytics.Application.Queries;
using CommerceHub.Analytics.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.TestBase;

namespace CommerceHub.Analytics.UnitTests;

public class GetSalesReportQueryHandlerTests
{
    private readonly Mock<IAnalyticsDbContext> _contextMock;
    private readonly GetSalesReportQueryHandler _handler;

    public GetSalesReportQueryHandlerTests()
    {
        _contextMock = new Mock<IAnalyticsDbContext>();
        _handler = new GetSalesReportQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnReport_WithCorrectTotals()
    {
        var now = DateTime.UtcNow;
        var events = new List<AnalyticsEvent>
        {
            new() { Id = 1, EventType = "PaymentConfirmed", EventData = "100.00", Timestamp = now.Date, UserId = 1 },
            new() { Id = 2, EventType = "PaymentConfirmed", EventData = "250.00", Timestamp = now.Date, UserId = 2 },
            new() { Id = 3, EventType = "OrderPlaced", Timestamp = now.Date, UserId = 1 },
            new() { Id = 4, EventType = "OrderPlaced", Timestamp = now.Date, UserId = 2 },
            new() { Id = 5, EventType = "PaymentConfirmed", EventData = "50.00", Timestamp = now.Date.AddDays(-1), UserId = 3 },
            new() { Id = 6, EventType = "OrderPlaced", Timestamp = now.Date.AddDays(-1), UserId = 3 }
        }.AsQueryable();

        var mockEvents = events.BuildMockDbSet();
        _contextMock.Setup(x => x.AnalyticsEvents).Returns(mockEvents.Object);

        var query = new GetSalesReportQuery
        {
            From = now.Date.AddDays(-1),
            To = now.Date.AddDays(1)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalSales.Should().Be(400.00m);
        result.TotalOrders.Should().Be(3);
        result.AvgOrderValue.Should().Be(400.00m / 3m);
        result.DailySales.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoEvents()
    {
        var events = new List<AnalyticsEvent>().AsQueryable();
        var mockEvents = events.BuildMockDbSet();
        _contextMock.Setup(x => x.AnalyticsEvents).Returns(mockEvents.Object);

        var query = new GetSalesReportQuery
        {
            From = DateTime.UtcNow.AddDays(-1),
            To = DateTime.UtcNow
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalSales.Should().Be(0);
        result.TotalOrders.Should().Be(0);
        result.AvgOrderValue.Should().Be(0);
        result.DailySales.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateRange()
    {
        var now = DateTime.UtcNow;
        var events = new List<AnalyticsEvent>
        {
            new() { Id = 1, EventType = "PaymentConfirmed", EventData = "100.00", Timestamp = now.Date, UserId = 1 },
            new() { Id = 2, EventType = "PaymentConfirmed", EventData = "200.00", Timestamp = now.Date.AddDays(-3), UserId = 2 },
            new() { Id = 3, EventType = "OrderPlaced", Timestamp = now.Date, UserId = 1 }
        }.AsQueryable();

        var mockEvents = events.BuildMockDbSet();
        _contextMock.Setup(x => x.AnalyticsEvents).Returns(mockEvents.Object);

        var query = new GetSalesReportQuery
        {
            From = now.Date.AddDays(-1),
            To = now.Date.AddDays(1)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalSales.Should().Be(100.00m);
        result.TotalOrders.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldGroupDailySalesCorrectly()
    {
        var now = DateTime.UtcNow;
        var events = new List<AnalyticsEvent>
        {
            new() { Id = 1, EventType = "PaymentConfirmed", EventData = "50.00", Timestamp = now.Date, UserId = 1 },
            new() { Id = 2, EventType = "PaymentConfirmed", EventData = "75.00", Timestamp = now.Date, UserId = 2 },
            new() { Id = 3, EventType = "OrderPlaced", Timestamp = now.Date, UserId = 1 },
            new() { Id = 4, EventType = "PaymentConfirmed", EventData = "30.00", Timestamp = now.Date.AddDays(-1), UserId = 3 },
            new() { Id = 5, EventType = "OrderPlaced", Timestamp = now.Date.AddDays(-1), UserId = 3 }
        }.AsQueryable();

        var mockEvents = events.BuildMockDbSet();
        _contextMock.Setup(x => x.AnalyticsEvents).Returns(mockEvents.Object);

        var query = new GetSalesReportQuery
        {
            From = now.Date.AddDays(-2),
            To = now.Date.AddDays(1)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.DailySales.Should().HaveCount(2);
        result.DailySales.First(d => d.Date == now.Date).Revenue.Should().Be(125.00m);
        result.DailySales.First(d => d.Date == now.Date).OrderCount.Should().Be(1);
        result.DailySales.First(d => d.Date == now.Date.AddDays(-1)).Revenue.Should().Be(30.00m);
        result.DailySales.First(d => d.Date == now.Date.AddDays(-1)).OrderCount.Should().Be(1);
    }
}
