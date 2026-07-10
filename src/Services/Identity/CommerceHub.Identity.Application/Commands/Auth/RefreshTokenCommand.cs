using MediatR;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Commands.Auth;

public record RefreshTokenCommand : IRequest<AuthResponse>
{
    public string RefreshToken { get; init; } = string.Empty;
}
