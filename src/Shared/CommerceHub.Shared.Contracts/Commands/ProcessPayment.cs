namespace CommerceHub.Shared.Contracts.Commands;

public record ProcessPayment
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Provider { get; init; } = "Stripe";
}
