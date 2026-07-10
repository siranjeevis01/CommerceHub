namespace CommerceHub.Payment.Application.Common.Models;

public record PaymentResult
{
    public bool Success { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public string? PaymentIntentId { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ClientSecret { get; init; }
    public string Status { get; init; } = string.Empty;
}
