namespace CommerceHub.Shared.Messaging.Events;

[Obsolete("Use CommerceHub.Shared.Contracts.Events.PaymentConfirmed instead. This class will be removed in a future version.")]
public class PaymentConfirmed
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public int? UserId { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public DateTime ConfirmedAt { get; set; }
}
