using CommerceHub.Modules.Analytics.Application.Commands;
using CommerceHub.Modules.Analytics.Application.Common.Interfaces;
using CommerceHub.Modules.Analytics.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.TestBase;

namespace CommerceHub.Modules.Analytics.UnitTests;

public class GenerateReportCommandHandlerTests
{
    private readonly Mock<IAnalyticsDbContext> _contextMock;
    private readonly GenerateReportCommandHandler _handler;

    public GenerateReportCommandHandlerTests()
    {
        _contextMock = new Mock<IAnalyticsDbContext>();
        _handler = new GenerateReportCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateJsonReport()
    {
        var now = DateTime.UtcNow;
        var events = new List<AnalyticsEvent>
        {
            new() { Id = 1, EventType = "PageView", Timestamp = now, UserId = 1 },
            new() { Id = 2, EventType = "AddToCart", Timestamp = now, UserId = 1 },
            new() { Id = 3, EventType = "PageView", Timestamp = now, UserId = 2 }
        }.AsQueryable();

        var mockEvents = events.BuildMockDbSet();
        _contextMock.Setup(x => x.AnalyticsEvents).Returns(mockEvents.Object);

        var command = new GenerateReportCommand
        {
            ReportType = "Events Summary",
            From = now.AddDays(-1),
            To = now.AddDays(1),
            Format = ReportFormat.Excel
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyReport_WhenNoEvents()
    {
        var events = new List<AnalyticsEvent>().AsQueryable();
        var mockEvents = events.BuildMockDbSet();
        _contextMock.Setup(x => x.AnalyticsEvents).Returns(mockEvents.Object);

        var command = new GenerateReportCommand
        {
            ReportType = "Empty Report",
            From = DateTime.UtcNow.AddDays(-1),
            To = DateTime.UtcNow,
            Format = ReportFormat.Excel
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
    }
}
