using MediatR;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Queries;

public record GetCouponByIdQuery : IRequest<CouponDto?>
{
    public int Id { get; init; }
}
