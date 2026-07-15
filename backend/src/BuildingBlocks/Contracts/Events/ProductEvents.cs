namespace CommerceHub.Shared.Contracts.Events;

public record ProductCreated
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int CategoryId { get; init; }
    public int VendorId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ProductUpdated
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int CategoryId { get; init; }
    public int VendorId { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record ProductDeleted
{
    public int ProductId { get; init; }
    public string SKU { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
}

public record ProductViewed
{
    public int ProductId { get; init; }
    public int? UserId { get; init; }
    public string? SessionId { get; init; }
    public DateTime ViewedAt { get; init; }
}
