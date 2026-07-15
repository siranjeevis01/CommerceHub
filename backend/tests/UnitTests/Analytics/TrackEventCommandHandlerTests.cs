using CommerceHub.Modules.Analytics.Application.Commands;
using CommerceHub.Modules.Analytics.Application.Common.Interfaces;
using CommerceHub.Modules.Analytics.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.TestBase;

namespace CommerceHub.Modules.Analytics.UnitTests;

public class TrackEventCommandHandlerTests
{
    private readonly Mock<IAnalyticsDbContext> _contextMock;
    private readonly TrackEventCommandHandler _handler;

    public TrackEventCommandHandlerTests()
    {
        _contextMock = new Mock<IAnalyticsDbContext>();
        _handler = new TrackEventCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateAnalyticsEvent()
    {
        var events = new List<AnalyticsEvent>().AsQueryable();
        var mockEvents = events.BuildMockDbSet();
        _contextMock.Setup(x => x.AnalyticsEvents).Returns(mockEvents.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new TrackEventCommand
        {
            EventType = "PageView",
            EventData = "{\"page\":\"/home\"}",
            UserId = 42,
            SessionId = "sess_abc",
            Timestamp = DateTime.UtcNow
        };

        AnalyticsEvent? captured = null;
        _contextMock.Setup(x => x.AnalyticsEvents.Add(It.IsAny<AnalyticsEvent>()))
            .Callback<AnalyticsEvent>(e => captured = e);

        await _handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.EventType.Should().Be("PageView");
        captured.UserId.Should().Be(42);
        captured.SessionId.Should().Be("sess_abc");
        _contextMock.Verify(x => x.AnalyticsEvents.Add(It.IsAny<AnalyticsEvent>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleDifferentEventTypes()
    {
        var events = new List<AnalyticsEvent>().AsQueryable();
        var mockEvents = events.BuildMockDbSet();
        _contextMock.Setup(x => x.AnalyticsEvents).Returns(mockEvents.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var types = new[] { "PageView", "ProductView", "AddToCart", "CheckoutStarted", "PaymentConfirmed", "OrderPlaced" };

        foreach (var eventType in types)
        {
            AnalyticsEvent? captured = null;
            _contextMock.Setup(x => x.AnalyticsEvents.Add(It.IsAny<AnalyticsEvent>()))
                .Callback<AnalyticsEvent>(e => captured = e);

            var command = new TrackEventCommand { EventType = eventType, Timestamp = DateTime.UtcNow };

            await _handler.Handle(command, CancellationToken.None);

            captured!.EventType.Should().Be(eventType);
        }
    }
}
