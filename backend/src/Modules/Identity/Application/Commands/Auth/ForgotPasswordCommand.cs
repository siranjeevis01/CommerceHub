using MediatR;

namespace CommerceHub.Modules.Identity.Application.Commands.Auth;

public record ForgotPasswordCommand : IRequest<Unit>
{
    public string Email { get; init; } = string.Empty;
}
