using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Analytics.Infrastructure.Data;

public class AnalyticsDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();
        var conn = Environment.GetEnvironmentVariable("ANALYTICS_DB_CONNECTION")
            ?? "server=localhost;port=5432;database=commercehub_analytics;user id=postgres;password=postgres";
        optionsBuilder.UseNpgsql(conn);
        return new AnalyticsDbContext(optionsBuilder.Options);
    }
}
