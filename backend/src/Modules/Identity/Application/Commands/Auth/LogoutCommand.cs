using MediatR;

namespace CommerceHub.Modules.Identity.Application.Commands.Auth;

public record LogoutCommand : IRequest<Unit>
{
    public string RefreshToken { get; init; } = string.Empty;
}
