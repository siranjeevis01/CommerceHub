using MediatR;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Commands;

public record ApplyCouponCommand : IRequest<CouponApplyResult>
{
    public string Code { get; init; } = string.Empty;
    public decimal OrderAmount { get; init; }
    public int UserId { get; init; }
}
