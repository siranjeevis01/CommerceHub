namespace CommerceHub.Shared.Messaging.Events;

[Obsolete("Use CommerceHub.Shared.Contracts.Events.PaymentRefunded instead. This class will be removed in a future version.")]
public class PaymentRefunded
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime RefundedAt { get; set; }
}
