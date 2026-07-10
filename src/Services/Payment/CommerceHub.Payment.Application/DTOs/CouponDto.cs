namespace CommerceHub.Payment.Application.DTOs;

public record CouponDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DiscountType { get; init; } = "Percentage";
    public decimal DiscountValue { get; init; }
    public decimal? MaxDiscountAmount { get; init; }
    public decimal? MinimumOrderAmount { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime ExpiryDate { get; init; }
    public int? UsageLimit { get; init; }
    public int? PerUserLimit { get; init; }
    public int CurrentUsageCount { get; init; }
    public bool IsActive { get; init; }
}

public record CouponApplyResult
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Discount { get; init; }
}
