namespace CommerceHub.Shared.Messaging.Events;

public class PaymentRefunded
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime RefundedAt { get; set; }
}
