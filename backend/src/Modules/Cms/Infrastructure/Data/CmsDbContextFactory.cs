using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Modules.Cms.Infrastructure.Data;

public class CmsDbContextFactory : IDesignTimeDbContextFactory<CmsDbContext>
{
    public CmsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CmsDbContext>();
        var conn = Environment.GetEnvironmentVariable("CMS_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_cms;user=root;password=root";
        optionsBuilder.UseMySQL(conn);
        return new CmsDbContext(optionsBuilder.Options);
    }
}
