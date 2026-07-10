using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Vendor.Infrastructure.Data;

public class VendorDbContextFactory : IDesignTimeDbContextFactory<VendorDbContext>
{
    public VendorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VendorDbContext>();
        var conn = Environment.GetEnvironmentVariable("VENDOR_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_vendor;user=root;password=root";
        optionsBuilder.UseMySQL(conn);
        return new VendorDbContext(optionsBuilder.Options);
    }
}
