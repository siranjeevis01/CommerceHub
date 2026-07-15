namespace CommerceHub.Modules.Payment.Application.DTOs;

public record PaymentIntentDto
{
    public int PaymentId { get; init; }
    public string? ClientSecret { get; init; }
    public string Status { get; init; } = string.Empty;
    public string TransactionId { get; init; } = string.Empty;
    public bool RequiresAction { get; init; }
}
