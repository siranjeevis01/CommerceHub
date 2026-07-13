using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Notification.Application.Common.Interfaces;
using CommerceHub.Notification.Infrastructure.Persistence;
using CommerceHub.Notification.Infrastructure.Repositories;
using CommerceHub.Notification.Infrastructure.Services;
using CommerceHub.Notification.Infrastructure.Hubs;

namespace CommerceHub.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("NOTIFICATION_DB_CONNECTION")
            ?? throw new InvalidOperationException("Notification database connection string is not configured.");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseMySQL(connectionString,
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));

        services.AddScoped<INotificationDbContext>(sp => sp.GetRequiredService<NotificationDbContext>());
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddHttpClient<IWhatsAppService, WhatsAppService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();
        services.AddScoped<IUserLookupService, UserLookupService>();
        services.AddSingleton<INotificationHub, NotificationService>();
        return services;
    }
}
