using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Inventory.Infrastructure.Data;

public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        var conn = Environment.GetEnvironmentVariable("INVENTORY_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_inventory;user=root;password=root";
        optionsBuilder.UseMySQL(conn);
        return new InventoryDbContext(optionsBuilder.Options);
    }
}
