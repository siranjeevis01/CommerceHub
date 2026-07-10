using MediatR;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Commands;

public record HandleWebhookCommand : IRequest<PaymentIntentDto>
{
    public string Provider { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string Signature { get; init; } = string.Empty;
}
