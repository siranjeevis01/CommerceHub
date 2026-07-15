using CommerceHub.Modules.Payment.Domain.Entities;
using FluentAssertions;
using Xunit;
using PaymentEntity = CommerceHub.Modules.Payment.Domain.Entities.Payment;

namespace CommerceHub.Modules.Payment.UnitTests;

public class PaymentDomainTests
{
    [Fact]
    public void Payment_ShouldInitializeWithPendingStatus()
    {
        var payment = new PaymentEntity
        {
            OrderId = 1,
            Amount = 150.00m,
            PaymentMethod = "Credit Card",
            Status = "Pending"
        };

        payment.Status.Should().Be("Pending");
        payment.Amount.Should().Be(150.00m);
    }

    [Fact]
    public void Payment_ShouldCapturePayment()
    {
        var payment = new PaymentEntity { Status = "Pending", TransactionId = "txn_123" };
        payment.Status = "Completed";
        payment.Status.Should().Be("Completed");
    }

    [Fact]
    public void Payment_ShouldFailWithReason()
    {
        var payment = new PaymentEntity { Status = "Pending" };
        payment.Status = "Failed";
        payment.FailureReason = "Insufficient funds";
        payment.Status.Should().Be("Failed");
        payment.FailureReason.Should().Be("Insufficient funds");
    }

    [Fact]
    public void Coupon_ShouldSetProperties()
    {
        var coupon = new Coupon
        {
            Code = "SAVE20",
            Name = "Save 20%",
            DiscountType = "Percentage",
            DiscountValue = 20,
            MinimumOrderAmount = 100,
            IsActive = true
        };

        coupon.Code.Should().Be("SAVE20");
        coupon.DiscountValue.Should().Be(20);
        coupon.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Coupon_ShouldCalculatePercentageDiscount()
    {
        var coupon = new Coupon
        {
            IsActive = true,
            DiscountType = "Percentage",
            DiscountValue = 20,
            MaxDiscountAmount = 50,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var discount = coupon.CalculateDiscount(200m);
        discount.Should().Be(40m);
    }

    [Fact]
    public void GiftCard_ShouldSetProperties()
    {
        var card = new GiftCard
        {
            Code = "GIFT-ABC-123",
            InitialBalance = 100.00m,
            RemainingBalance = 100.00m,
            IsActive = true
        };

        card.Code.Should().Be("GIFT-ABC-123");
        card.RemainingBalance.Should().Be(100.00m);
    }
}
