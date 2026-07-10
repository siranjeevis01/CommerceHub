namespace CommerceHub.Shared.Messaging.Events;

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
