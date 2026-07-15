namespace CommerceHub.Modules.Inventory.Domain.Entities;

public class Inventory : BaseEntity
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int WarehouseId { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public int LowStockThreshold { get; set; } = 10;
    public Warehouse Warehouse { get; set; } = null!;
}
