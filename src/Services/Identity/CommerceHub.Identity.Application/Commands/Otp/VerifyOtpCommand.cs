using MediatR;

namespace CommerceHub.Identity.Application.Commands.Otp;

public record VerifyOtpCommand : IRequest
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
