using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Modules.Analytics.Infrastructure.Data;

public class AnalyticsDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();
        var conn = Environment.GetEnvironmentVariable("ANALYTICS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=commercehub_analytics;Username=postgres;Password=";
        optionsBuilder.UseNpgsql(conn);
        return new AnalyticsDbContext(optionsBuilder.Options);
    }
}
