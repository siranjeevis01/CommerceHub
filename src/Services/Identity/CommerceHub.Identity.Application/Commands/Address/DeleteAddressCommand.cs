using MediatR;

namespace CommerceHub.Identity.Application.Commands.Address;

public record DeleteAddressCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
