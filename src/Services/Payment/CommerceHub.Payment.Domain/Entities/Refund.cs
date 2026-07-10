namespace CommerceHub.Payment.Domain.Entities;

public class Refund : BaseEntity
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? RefundTransactionId { get; set; }
    public string Status { get; set; } = "Pending";
    public Payment? Payment { get; set; }
}
