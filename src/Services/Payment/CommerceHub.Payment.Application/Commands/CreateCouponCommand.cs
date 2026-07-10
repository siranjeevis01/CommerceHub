using MediatR;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Commands;

public record CreateCouponCommand : IRequest<CouponDto>
{
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
}
