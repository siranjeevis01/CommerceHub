using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Inventory.Domain.Entities;
using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using InventoryEntity = CommerceHub.Modules.Inventory.Domain.Entities.Inventory;

namespace CommerceHub.Modules.Inventory.Infrastructure.Data;

public class InventoryDbContext : DbContext, IInventoryDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }
    public DbSet<InventoryEntity> Inventories { get; set; }
    public DbSet<InventoryTransaction> StockMovements { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<InventoryEntity>().HasIndex(i => new { i.ProductId, i.VariantId, i.WarehouseId }).IsUnique();
        modelBuilder.Entity<InventoryEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InventoryTransaction>().ToTable("InventoryTransactions");
    }
}
