using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Order.Infrastructure.Data;

public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        var conn = Environment.GetEnvironmentVariable("ORDER_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_order;user=root;password=root";
        optionsBuilder.UseMySQL(conn);
        return new OrderDbContext(optionsBuilder.Options);
    }
}
