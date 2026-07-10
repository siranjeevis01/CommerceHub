using CommerceHub.Vendor.Infrastructure.Data;
using CommerceHub.Vendor.Domain.Entities;
using CommerceHub.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommerceHub.Vendor.IntegrationTests;

public class VendorDbContextTests : IntegrationTestBase
{
    [Fact]
    public async Task CanCreateAndRetrieveVendorProfile()
    {
        var options = new DbContextOptionsBuilder<VendorDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new VendorDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var profile = new VendorProfile
        {
            UserId = 1,
            StoreName = "Integration Test Store",
            StoreDescription = "Created during integration test",
            BusinessEmail = "vendor@integrationtest.com",
            BusinessPhone = "+1234567890",
            VerificationStatus = "Pending"
        };

        context.Vendors.Add(profile);
        await context.SaveChangesAsync();

        var retrieved = await context.Vendors.FirstOrDefaultAsync(p => p.StoreName == "Integration Test Store");
        retrieved.Should().NotBeNull();
        retrieved!.StoreName.Should().Be("Integration Test Store");
        retrieved.VerificationStatus.Should().Be("Pending");
    }

    [Fact]
    public async Task CanCreatePayout()
    {
        var options = new DbContextOptionsBuilder<VendorDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new VendorDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var profile = new VendorProfile
        {
            UserId = 2,
            StoreName = "Payout Test Store",
            BusinessEmail = "payout@test.com",
            VerificationStatus = "Approved"
        };

        context.Vendors.Add(profile);
        await context.SaveChangesAsync();

        var payout = new VendorPayout
        {
            VendorId = profile.Id,
            PayoutNumber = "PO-001",
            Amount = 500.00m,
            Status = "Pending"
        };

        context.Payouts.Add(payout);
        await context.SaveChangesAsync();

        var retrieved = await context.Payouts.FirstOrDefaultAsync(p => p.VendorId == profile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Amount.Should().Be(500.00m);
    }
}
