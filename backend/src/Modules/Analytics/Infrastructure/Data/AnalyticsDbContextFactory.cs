using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Modules.Analytics.Infrastructure.Data;

public class AnalyticsDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();
        var conn = Environment.GetEnvironmentVariable("ANALYTICS_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_analytics;user=root;password=root";
        if (!conn.Contains("SslMode")) conn += ";SslMode=Required;AllowPublicKeyRetrieval=true";
        optionsBuilder.UseMySQL(conn);
        return new AnalyticsDbContext(optionsBuilder.Options);
    }
}
