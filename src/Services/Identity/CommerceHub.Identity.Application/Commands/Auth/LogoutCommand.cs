using MediatR;

namespace CommerceHub.Identity.Application.Commands.Auth;

public record LogoutCommand : IRequest<Unit>
{
    public string RefreshToken { get; init; } = string.Empty;
}
