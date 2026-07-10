namespace CommerceHub.Payment.Application.DTOs;

public record PaymentDto
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string? TransactionId { get; init; }
    public string? PaymentIntentId { get; init; }
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public DateTime CreatedAt { get; init; }
}
