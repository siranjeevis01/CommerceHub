namespace CommerceHub.Modules.Payment.Domain.Entities;

public class Payment : BaseEntity
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? PaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? FailureReason { get; set; }
    public string? PaymentDetails { get; set; }
    public int OrderId { get; set; }
}
