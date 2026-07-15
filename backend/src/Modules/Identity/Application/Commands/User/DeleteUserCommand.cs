using MediatR;

namespace CommerceHub.Modules.Identity.Application.Commands.User;

public record DeleteUserCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
