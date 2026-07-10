using MediatR;

namespace CommerceHub.Identity.Application.Commands.Auth;

public record VerifyEmailCommand : IRequest<Unit>
{
    public string Email { get; init; } = string.Empty;
    public string OtpCode { get; init; } = string.Empty;
}
