using CommerceHub.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Inventory.Application.Common.Interfaces;

public interface IInventoryDbContext
{
    DbSet<Domain.Entities.Inventory> Inventories { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<InventoryTransaction> StockMovements { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
