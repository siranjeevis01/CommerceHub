namespace CommerceHub.Modules.Inventory.Domain.Entities;

public class InventoryTransaction : BaseEntity
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int QuantityChange { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public int? OrderId { get; set; }
}
