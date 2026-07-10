using MediatR;

namespace CommerceHub.Identity.Application.Commands.User;

public record DeleteUserCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
