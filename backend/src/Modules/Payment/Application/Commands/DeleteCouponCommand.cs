using MediatR;

namespace CommerceHub.Modules.Payment.Application.Commands;

public record DeleteCouponCommand : IRequest
{
    public int Id { get; init; }
}
