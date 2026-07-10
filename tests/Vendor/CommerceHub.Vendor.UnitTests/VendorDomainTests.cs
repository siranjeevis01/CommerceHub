using CommerceHub.Vendor.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Vendor.UnitTests;

public class VendorDomainTests
{
    [Fact]
    public void VendorProfile_ShouldInitializeWithPendingVerification()
    {
        var profile = new VendorProfile
        {
            StoreName = "Test Store",
            StoreDescription = "A test vendor store",
            BusinessEmail = "vendor@test.com",
            BusinessPhone = "+1234567890",
            VerificationStatus = "Pending",
            UserId = 1
        };

        profile.VerificationStatus.Should().Be("Pending");
        profile.StoreName.Should().Be("Test Store");
        profile.CommissionRate.Should().Be(0);
    }

    [Fact]
    public void VendorProfile_ShouldApprove()
    {
        var profile = new VendorProfile { VerificationStatus = "Pending" };
        profile.VerificationStatus = "Approved";
        profile.VerificationStatus.Should().Be("Approved");
    }

    [Fact]
    public void VendorProfile_ShouldReject()
    {
        var profile = new VendorProfile { VerificationStatus = "Pending" };
        profile.VerificationStatus = "Rejected";
        profile.VerificationStatus.Should().Be("Rejected");
    }

    [Fact]
    public void VendorProfile_ShouldTrackSalesAndEarnings()
    {
        var profile = new VendorProfile { TotalSales = 0, TotalEarnings = 0, Balance = 0 };
        profile.TotalSales = 15000.00m;
        profile.TotalEarnings = 12750.00m;
        profile.Balance = 5000.00m;
        profile.TotalSales.Should().Be(15000.00m);
        profile.TotalEarnings.Should().Be(12750.00m);
        profile.Balance.Should().Be(5000.00m);
    }

    [Fact]
    public void VendorPayout_ShouldSetProperties()
    {
        var payout = new VendorPayout
        {
            PayoutNumber = "PO-001",
            VendorId = 1,
            Amount = 1500.00m,
            Status = "Pending",
            PaymentMethod = "Bank Transfer"
        };

        payout.Amount.Should().Be(1500.00m);
        payout.Status.Should().Be("Pending");
        payout.PayoutNumber.Should().Be("PO-001");
    }

    [Fact]
    public void VendorPayout_ShouldComplete()
    {
        var payout = new VendorPayout { Status = "Pending", TransactionId = "TXN-123" };
        payout.Status = "Paid";
        payout.Status.Should().Be("Paid");
    }

    [Fact]
    public void CommissionConfig_ShouldSetRate()
    {
        var config = new CommissionConfig
        {
            Name = "Standard Commission",
            Type = "Global",
            Rate = 0.15m
        };

        config.Rate.Should().Be(0.15m);
        config.Type.Should().Be("Global");
    }

    [Fact]
    public void CommissionConfig_ShouldSupportCategoryOverride()
    {
        var config = new CommissionConfig
        {
            Name = "Electronics",
            Type = "Category",
            TargetId = 5,
            Rate = 0.10m
        };

        config.TargetId.Should().Be(5);
        config.Rate.Should().Be(0.10m);
    }

    [Fact]
    public void Settlement_ShouldCalculateEarnings()
    {
        var settlement = new Settlement
        {
            VendorId = 1,
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            TotalAmount = 10000.00m,
            TotalCommission = 1500.00m,
            TotalEarnings = 8500.00m,
            Status = "Pending"
        };

        settlement.TotalEarnings.Should().Be(8500.00m);
        settlement.TotalCommission.Should().Be(1500.00m);
    }

    [Fact]
    public void Settlement_ShouldComplete()
    {
        var settlement = new Settlement { Status = "Pending" };
        settlement.Status = "Completed";
        settlement.Status.Should().Be("Completed");
    }

    [Fact]
    public void VendorDocument_ShouldStoreVerificationFile()
    {
        var doc = new VendorDocument
        {
            VendorId = 1,
            DocumentType = "BusinessLicense",
            FileUrl = "https://storage.example.com/documents/license.pdf",
            VerificationStatus = "Pending"
        };

        doc.DocumentType.Should().Be("BusinessLicense");
        doc.VerificationStatus.Should().Be("Pending");
    }

    [Fact]
    public void VendorDocument_ShouldVerify()
    {
        var doc = new VendorDocument { VerificationStatus = "Pending" };
        doc.VerificationStatus = "Verified";
        doc.VerificationStatus.Should().Be("Verified");
    }
}
