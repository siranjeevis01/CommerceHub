namespace CommerceHub.Payment.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "Percentage";
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int? UsageLimit { get; set; }
    public int? PerUserLimit { get; set; }
    public int CurrentUsageCount { get; set; }

    public bool IsValid()
    {
        if (!IsActive) return false;
        if (UsageLimit.HasValue && CurrentUsageCount >= UsageLimit.Value) return false;
        if (DateTime.UtcNow < StartDate) return false;
        if (DateTime.UtcNow > ExpiryDate) return false;
        return true;
    }

    public bool IsValidForOrder(decimal orderAmount)
    {
        if (!IsValid()) return false;
        if (MinimumOrderAmount.HasValue && orderAmount < MinimumOrderAmount.Value) return false;
        return true;
    }

    public decimal CalculateDiscount(decimal orderAmount)
    {
        if (!IsValidForOrder(orderAmount)) return 0;

        var discount = DiscountType switch
        {
            "Percentage" => orderAmount * DiscountValue / 100m,
            "Fixed" => DiscountValue,
            _ => 0
        };

        if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
            discount = MaxDiscountAmount.Value;

        return Math.Min(discount, orderAmount);
    }
}
