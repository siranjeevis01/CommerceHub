using CommerceHub.Modules.Analytics.Infrastructure.Data;
using CommerceHub.Modules.Analytics.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommerceHub.Modules.Analytics.IntegrationTests;

public class AnalyticsDbContextTests
{
    private static AnalyticsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AnalyticsDbContext(options);
    }

    [Fact]
    public async Task CanCreateAndRetrieveSalesReport()
    {
        using var context = CreateContext();

        var report = new SalesReport
        {
            ReportDate = DateTime.UtcNow,
            Period = "Daily",
            TotalSales = 15000.00m,
            TotalOrders = 100,
            TotalCommission = 750.00m,
            TotalEarnings = 14250.00m,
            NewUsers = 25,
            NewVendors = 3
        };

        context.SalesReports.Add(report);
        await context.SaveChangesAsync();

        var retrieved = await context.SalesReports.FirstOrDefaultAsync(r => r.TotalOrders == 100);
        retrieved.Should().NotBeNull();
        retrieved!.TotalSales.Should().Be(15000.00m);
    }

    [Fact]
    public async Task CanCreateAndRetrieveAuditLog()
    {
        using var context = CreateContext();

        var log = new AuditLog
        {
            Action = "ProductUpdated",
            Entity = "Product",
            EntityId = "99",
            UserId = 5,
            UserRole = "Vendor",
            IpAddress = "10.0.0.1"
        };

        context.AuditLogs.Add(log);
        await context.SaveChangesAsync();

        var retrieved = await context.AuditLogs.FirstOrDefaultAsync(l => l.Action == "ProductUpdated");
        retrieved.Should().NotBeNull();
        retrieved!.Entity.Should().Be("Product");
        retrieved.UserRole.Should().Be("Vendor");
    }
}
