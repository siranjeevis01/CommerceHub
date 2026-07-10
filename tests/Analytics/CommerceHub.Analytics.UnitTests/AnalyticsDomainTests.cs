using CommerceHub.Analytics.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Analytics.UnitTests;

public class AnalyticsDomainTests
{
    [Fact]
    public void SalesReport_ShouldInitializeWithDefaults()
    {
        var report = new SalesReport
        {
            Id = 1,
            ReportDate = DateTime.UtcNow,
            Period = "Daily",
            TotalSales = 5000.00m,
            TotalOrders = 50,
            TotalCommission = 250.00m,
            TotalEarnings = 4750.00m,
            NewUsers = 15,
            NewVendors = 2
        };

        report.TotalOrders.Should().Be(50);
        report.TotalSales.Should().Be(5000.00m);
        report.TotalEarnings.Should().Be(4750.00m);
    }

    [Fact]
    public void SalesReport_GeneratedAt_ShouldBeUtcNow()
    {
        var report = new SalesReport();
        report.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AuditLog_ShouldInitializeWithProperties()
    {
        var log = new AuditLog
        {
            Action = "UserLogin",
            Entity = "User",
            EntityId = "42",
            OldValues = null,
            NewValues = "{\"lastLogin\":\"2026-07-08\"}",
            UserId = 1,
            UserRole = "Admin",
            IpAddress = "192.168.1.1"
        };

        log.Action.Should().Be("UserLogin");
        log.Entity.Should().Be("User");
        log.UserRole.Should().Be("Admin");
    }

    [Fact]
    public void AuditLog_Timestamp_ShouldDefaultToUtcNow()
    {
        var log = new AuditLog();
        log.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
