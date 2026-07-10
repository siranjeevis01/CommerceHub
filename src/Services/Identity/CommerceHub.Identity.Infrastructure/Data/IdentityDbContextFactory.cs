using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Identity.Infrastructure.Data;

public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        var conn = Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_identity;user=root;password=root";
        optionsBuilder.UseMySQL(conn);
        return new IdentityDbContext(optionsBuilder.Options);
    }
}
