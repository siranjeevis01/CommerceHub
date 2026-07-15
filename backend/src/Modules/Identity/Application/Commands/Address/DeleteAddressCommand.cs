using MediatR;

namespace CommerceHub.Modules.Identity.Application.Commands.Address;

public record DeleteAddressCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
