namespace CommerceHub.Modules.Cms.Application.DTOs;

public record CouponDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal? DiscountAmount { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public decimal? MinimumOrderAmount { get; init; }
    public int? MaxUsageCount { get; init; }
    public int UsedCount { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public bool IsActive { get; init; }
}
