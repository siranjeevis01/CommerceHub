namespace CommerceHub.Shared.Contracts.Events;

public record ProcessPayment
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Provider { get; init; } = "Stripe";
}

public record PaymentConfirmed
{
    public int PaymentId { get; init; }
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime ConfirmedAt { get; init; }
}

public record PaymentFailed
{
    public int PaymentId { get; init; }
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public string FailureReason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
}

public record PaymentRefunded
{
    public int PaymentId { get; init; }
    public int OrderId { get; init; }
    public decimal RefundAmount { get; init; }
    public DateTime RefundedAt { get; init; }
}

public record CouponApplied
{
    public int CouponId { get; init; }
    public string Code { get; init; } = string.Empty;
    public int UserId { get; init; }
    public decimal DiscountAmount { get; init; }
}
