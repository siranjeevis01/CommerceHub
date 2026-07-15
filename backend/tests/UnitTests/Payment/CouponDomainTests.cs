using FluentAssertions;
using Xunit;
using CouponEntity = CommerceHub.Modules.Payment.Domain.Entities.Coupon;

namespace CommerceHub.Modules.Payment.UnitTests;

public class CouponDomainTests
{
    [Fact]
    public void IsValid_ShouldReturnTrue_WhenCouponIsActiveAndWithinDateRange()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            UsageLimit = null,
            CurrentUsageCount = 0
        };

        var result = coupon.IsValid();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenCouponInactive()
    {
        var coupon = new CouponEntity
        {
            IsActive = false,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var result = coupon.IsValid();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenExpired()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-10),
            ExpiryDate = DateTime.UtcNow.AddDays(-1)
        };

        var result = coupon.IsValid();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenNotYetStarted()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(1),
            ExpiryDate = DateTime.UtcNow.AddDays(10)
        };

        var result = coupon.IsValid();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenUsageLimitExceeded()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            UsageLimit = 10,
            CurrentUsageCount = 10
        };

        var result = coupon.IsValid();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidForOrder_ShouldReturnFalse_WhenBelowMinimumOrder()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            MinimumOrderAmount = 100
        };

        var result = coupon.IsValidForOrder(50m);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidForOrder_ShouldReturnTrue_WhenAboveMinimumOrder()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            MinimumOrderAmount = 100
        };

        var result = coupon.IsValidForOrder(150m);

        result.Should().BeTrue();
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnPercentageOfOrder()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            DiscountType = "Percentage",
            DiscountValue = 20,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var discount = coupon.CalculateDiscount(200m);

        discount.Should().Be(40m);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnFixedAmount()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            DiscountType = "Fixed",
            DiscountValue = 50,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var discount = coupon.CalculateDiscount(200m);

        discount.Should().Be(50m);
    }

    [Fact]
    public void CalculateDiscount_ShouldCapAtMaxDiscountAmount()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            DiscountType = "Percentage",
            DiscountValue = 50,
            MaxDiscountAmount = 25,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var discount = coupon.CalculateDiscount(200m);

        discount.Should().Be(25m);
    }

    [Fact]
    public void CalculateDiscount_ShouldNotExceedOrderAmount()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            DiscountType = "Fixed",
            DiscountValue = 200,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var discount = coupon.CalculateDiscount(50m);

        discount.Should().Be(50m);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnZero_WhenCouponInvalid()
    {
        var coupon = new CouponEntity
        {
            IsActive = false,
            DiscountType = "Percentage",
            DiscountValue = 20,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var discount = coupon.CalculateDiscount(200m);

        discount.Should().Be(0);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnZero_WhenDiscountTypeUnknown()
    {
        var coupon = new CouponEntity
        {
            IsActive = true,
            DiscountType = "Unknown",
            DiscountValue = 20,
            StartDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        var discount = coupon.CalculateDiscount(200m);

        discount.Should().Be(0);
    }
}
