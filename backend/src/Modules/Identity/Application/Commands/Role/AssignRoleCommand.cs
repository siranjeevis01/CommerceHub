using MediatR;

namespace CommerceHub.Modules.Identity.Application.Commands.Role;

public record AssignRoleCommand : IRequest
{
    public int UserId { get; init; }
    public int RoleId { get; init; }
}
