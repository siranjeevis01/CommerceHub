using CommerceHub.Modules.Inventory.Domain.Entities;
using FluentAssertions;
using Xunit;
using InventoryEntity = CommerceHub.Modules.Inventory.Domain.Entities.Inventory;

namespace CommerceHub.Modules.Inventory.UnitTests;

public class InventoryDomainTests
{
    [Fact]
    public void Inventory_ShouldInitializeWithDefaultValues()
    {
        var item = new InventoryEntity
        {
            ProductId = 1,
            WarehouseId = 1,
            StockQuantity = 100,
            ReservedQuantity = 10,
            LowStockThreshold = 20
        };

        item.AvailableQuantity.Should().Be(90);
    }

    [Fact]
    public void Inventory_ShouldBeLowStock_WhenAvailableBelowThreshold()
    {
        var item = new InventoryEntity
        {
            ProductId = 1,
            WarehouseId = 1,
            StockQuantity = 15,
            ReservedQuantity = 0,
            LowStockThreshold = 20
        };

        item.AvailableQuantity.Should().Be(15);
        (item.AvailableQuantity < item.LowStockThreshold).Should().BeTrue();
    }

    [Fact]
    public void InventoryTransaction_ShouldSetProperties()
    {
        var tx = new InventoryTransaction
        {
            InventoryId = 1,
            ProductId = 1,
            QuantityChange = -5,
            TransactionType = "Sale",
            Reference = "ORD-123",
            OrderId = 123
        };

        tx.QuantityChange.Should().Be(-5);
        tx.TransactionType.Should().Be("Sale");
        tx.OrderId.Should().Be(123);
    }

    [Fact]
    public void InventoryTransaction_ShouldSupportRestock()
    {
        var tx = new InventoryTransaction
        {
            ProductId = 1,
            QuantityChange = 50,
            TransactionType = "Restock",
            Reference = "PO-456"
        };

        tx.QuantityChange.Should().Be(50);
        tx.TransactionType.Should().Be("Restock");
    }

    [Fact]
    public void Warehouse_ShouldInitializeWithProperties()
    {
        var warehouse = new Warehouse
        {
            Name = "Main Warehouse",
            Code = "WH-001",
            Address = "123 Storage Ave",
            City = "New York",
            Country = "USA"
        };

        warehouse.Name.Should().Be("Main Warehouse");
        warehouse.Code.Should().Be("WH-001");
    }

    [Fact]
    public void Inventory_ShouldCalculateAvailableQuantityCorrectly()
    {
        var item = new InventoryEntity
        {
            ProductId = 1,
            WarehouseId = 1,
            StockQuantity = 50,
            ReservedQuantity = 15,
            LowStockThreshold = 10
        };

        item.AvailableQuantity.Should().Be(35);
        item.ReservedQuantity = 20;
        item.AvailableQuantity.Should().Be(30);
    }
}
