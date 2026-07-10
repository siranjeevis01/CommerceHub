namespace CommerceHub.Shared.Messaging.Events;

public class PaymentFailed
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public int? UserId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime FailedAt { get; set; }
}
