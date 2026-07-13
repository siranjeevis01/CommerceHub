using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Payment.Application.Common.Interfaces;
using CommerceHub.Payment.Infrastructure.Data;
using CommerceHub.Payment.Infrastructure.Services;

namespace CommerceHub.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("PAYMENT_DB_CONNECTION")
            ?? throw new InvalidOperationException("Payment database connection string missing");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseMySQL(connectionString,
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(5)));

        services.AddScoped<IPaymentDbContext>(sp => sp.GetRequiredService<PaymentDbContext>());

        services.AddHttpClient<WhatsAppPaymentService>();
        services.AddScoped<StripePaymentGateway>();
        services.AddScoped<RazorpayPaymentGateway>();
        services.AddScoped<UpiQrPaymentGateway>();
        services.AddScoped<WhatsAppPaymentService>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

        return services;
    }
}
