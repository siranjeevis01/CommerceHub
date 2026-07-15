using MediatR;

namespace CommerceHub.Modules.Identity.Application.Commands.Otp;

public record SendOtpCommand : IRequest
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Type { get; init; }
    public string? DeliveryMethod { get; init; }
}
