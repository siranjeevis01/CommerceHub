using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;
using CommerceHub.Modules.Notification.Infrastructure.Persistence;
using CommerceHub.Modules.Notification.Infrastructure.Repositories;
using CommerceHub.Modules.Notification.Infrastructure.Services;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;

namespace CommerceHub.Modules.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Notification")
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
