namespace CommerceHub.Shared.Messaging.Events;

[Obsolete("Use CommerceHub.Shared.Contracts.Events.PaymentFailed instead. This class will be removed in a future version.")]
public class PaymentFailed
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public int? UserId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime FailedAt { get; set; }
}
