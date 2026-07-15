using MediatR;

namespace CommerceHub.Modules.Identity.Application.Commands.Auth;

public record ResetPasswordCommand : IRequest<Unit>
{
    public string Email { get; init; } = string.Empty;
    public string OtpCode { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
