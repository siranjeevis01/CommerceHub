using MediatR;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public record GetCouponByIdQuery : IRequest<CouponDto?>
{
    public int Id { get; init; }
}
