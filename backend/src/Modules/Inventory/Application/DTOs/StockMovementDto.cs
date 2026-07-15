namespace CommerceHub.Modules.Inventory.Application.DTOs;

public class StockMovementDto
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int QuantityChange { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public int? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
