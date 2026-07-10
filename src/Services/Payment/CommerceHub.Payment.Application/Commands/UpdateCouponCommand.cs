using MediatR;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Commands;

public record UpdateCouponCommand : IRequest<CouponDto>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal DiscountValue { get; init; }
    public DateTime ExpiryDate { get; init; }
    public int? UsageLimit { get; init; }
    public bool IsActive { get; init; }
}
