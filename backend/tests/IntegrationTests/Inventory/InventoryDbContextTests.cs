using CommerceHub.Modules.Inventory.Infrastructure.Data;
using CommerceHub.Modules.Inventory.Domain.Entities;
using CommerceHub.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InventoryEntity = CommerceHub.Modules.Inventory.Domain.Entities.Inventory;

namespace CommerceHub.Modules.Inventory.IntegrationTests;

public class InventoryDbContextTests : IntegrationTestBase
{
    [Fact]
    public async Task CanCreateAndRetrieveInventoryItem()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new InventoryDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var item = new InventoryEntity
        {
            ProductId = 1,
            WarehouseId = 1,
            StockQuantity = 100,
            ReservedQuantity = 0,
            LowStockThreshold = 20
        };

        context.Inventories.Add(item);
        await context.SaveChangesAsync();

        var retrieved = await context.Inventories.FirstOrDefaultAsync(i => i.ProductId == 1);
        retrieved.Should().NotBeNull();
        retrieved!.StockQuantity.Should().Be(100);
    }

    [Fact]
    public async Task CanCreateWarehouse()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new InventoryDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var warehouse = new Warehouse
        {
            Name = "Test Warehouse",
            Code = "TWH-001",
            Address = "123 Warehouse Blvd"
        };

        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        var retrieved = await context.Warehouses.FirstOrDefaultAsync(w => w.Code == "TWH-001");
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Warehouse");
    }
}
