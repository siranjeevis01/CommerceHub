using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Modules.Notification.Infrastructure.Persistence;

public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        var conn = Environment.GetEnvironmentVariable("NOTIFICATION_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_notification;user=root;password=root";
        if (!conn.Contains("SslMode")) conn += ";SslMode=Required;AllowPublicKeyRetrieval=true";
        optionsBuilder.UseMySQL(conn);
        return new NotificationDbContext(optionsBuilder.Options);
    }
}
