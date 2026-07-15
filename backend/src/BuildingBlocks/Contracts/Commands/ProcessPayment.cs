namespace CommerceHub.Shared.Contracts.Commands;

[Obsolete("Use CommerceHub.Shared.Contracts.Events.ProcessPayment instead. This record will be removed in a future version.")]
public record ProcessPaymentCommand
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Provider { get; init; } = "Stripe";
}
