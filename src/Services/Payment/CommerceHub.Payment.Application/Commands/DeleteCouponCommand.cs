using MediatR;

namespace CommerceHub.Payment.Application.Commands;

public record DeleteCouponCommand : IRequest
{
    public int Id { get; init; }
}
