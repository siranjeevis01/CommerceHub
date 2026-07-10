using MediatR;

namespace CommerceHub.Identity.Application.Commands.Otp;

public record SendOtpCommand : IRequest
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Type { get; init; }
    public string? DeliveryMethod { get; init; }
}
