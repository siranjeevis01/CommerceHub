namespace CommerceHub.Shared.Contracts.Events;

public record InventoryReserved
{
    public int OrderId { get; init; }
    public List<ReservedItem> Items { get; init; } = new();
    public DateTime ReservedAt { get; init; }
}

public record ReservedItem
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
}

public record InventoryFailed
{
    public int OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public List<FailedItem> FailedItems { get; init; } = new();
    public DateTime FailedAt { get; init; }
}

public record FailedItem
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int RequestedQuantity { get; init; }
    public int AvailableQuantity { get; init; }
}

public record StockDeducted
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
    public int RemainingStock { get; init; }
    public DateTime DeductedAt { get; init; }
}

public record StockAdded
{
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
    public int NewStock { get; init; }
    public DateTime AddedAt { get; init; }
}

public record LowStockAlert
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int CurrentStock { get; init; }
    public int Threshold { get; init; }
    public DateTime AlertedAt { get; init; }
}
