using MediatR;
using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Application.Commands.Auth;

public record RegisterUserCommand : IRequest<AuthResponse>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
