using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Product.Infrastructure.Data;

public class ProductDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();
        var conn = Environment.GetEnvironmentVariable("PRODUCT_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_product;user=root;password=root";
        optionsBuilder.UseMySQL(conn);
        return new ProductDbContext(optionsBuilder.Options);
    }
}
