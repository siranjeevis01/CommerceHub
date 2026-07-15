using MediatR;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Commands;

public record ProcessPaymentCommand : IRequest<PaymentIntentDto>
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Provider { get; init; } = "Stripe";
    public string PaymentMethodId { get; init; } = string.Empty;
    public string? ReturnUrl { get; init; }
}
