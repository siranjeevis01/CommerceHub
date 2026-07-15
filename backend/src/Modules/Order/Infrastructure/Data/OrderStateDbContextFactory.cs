using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Modules.Order.Infrastructure.Data;

public class OrderStateDbContextFactory : IDesignTimeDbContextFactory<OrderStateDbContext>
{
    public OrderStateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderStateDbContext>();
        var conn = Environment.GetEnvironmentVariable("ORDER_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_order;user=root;password=root";
        optionsBuilder.UseMySQL(conn);
        return new OrderStateDbContext(optionsBuilder.Options);
    }
}
