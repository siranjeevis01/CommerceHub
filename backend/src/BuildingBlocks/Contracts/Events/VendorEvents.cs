namespace CommerceHub.Shared.Contracts.Events;

public record VendorCreated
{
    public int VendorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int OwnerUserId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record VendorUpdated
{
    public int VendorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}

public record VendorActivated
{
    public int VendorId { get; init; }
    public DateTime ActivatedAt { get; init; }
}

public record VendorDeactivated
{
    public int VendorId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime DeactivatedAt { get; init; }
}

public record VendorPayoutCompleted
{
    public int VendorId { get; init; }
    public int PayoutId { get; init; }
    public string PayoutNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime CompletedAt { get; init; }
}

public record VendorSettled
{
    public int VendorId { get; init; }
    public decimal Amount { get; init; }
    public DateTime SettledAt { get; init; }
}
