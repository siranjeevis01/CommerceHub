using MediatR;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Commands;

public record UpdateCouponCommand : IRequest<CouponDto>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal DiscountValue { get; init; }
    public DateTime ExpiryDate { get; init; }
    public int? UsageLimit { get; init; }
    public bool IsActive { get; init; }
}
