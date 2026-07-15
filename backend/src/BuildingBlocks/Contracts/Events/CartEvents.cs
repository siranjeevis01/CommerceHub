namespace CommerceHub.Shared.Contracts.Events;

public record CartUpdated
{
    public int CartId { get; init; }
    public int UserId { get; init; }
    public List<CartItemEvent> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CartItemEvent
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

public record CartCleared
{
    public int CartId { get; init; }
    public int UserId { get; init; }
    public DateTime ClearedAt { get; init; }
}
