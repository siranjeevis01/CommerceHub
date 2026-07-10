using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _smtpHost = configuration["Smtp:Host"] ?? Environment.GetEnvironmentVariable("SMTP_HOST") ?? "localhost";
        _smtpPort = int.Parse(configuration["Smtp:Port"] ?? Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _smtpUser = configuration["Smtp:User"] ?? Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
        _smtpPass = configuration["Smtp:Pass"] ?? Environment.GetEnvironmentVariable("SMTP_PASS") ?? "";
        _fromEmail = configuration["Smtp:FromEmail"] ?? Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "noreply@commercehub.com";
        _fromName = configuration["Smtp:FromName"] ?? Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "CommerceHub";
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _smtpPort == 587 || _smtpPort == 465,
                Credentials = !string.IsNullOrEmpty(_smtpUser)
                    ? new NetworkCredential(_smtpUser, _smtpPass)
                    : null
            };

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(to);

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
            return false;
        }
    }

    public async Task<bool> SendEmailWithTemplateAsync(string to, string templateName, Dictionary<string, string> templateData, CancellationToken cancellationToken = default)
    {
        var templates = new Dictionary<string, (string Subject, string Body)>
        {
            ["order.confirmed"] = (
                "Order #{OrderNumber} Confirmed",
                "<h2>Order Confirmed</h2><p>Your order <strong>#{OrderNumber}</strong> has been confirmed.</p><p>Total: ${TotalAmount}</p>"
            ),
            ["order.shipped"] = (
                "Order #{OrderNumber} Shipped",
                "<h2>Order Shipped</h2><p>Your order <strong>#{OrderNumber}</strong> has been shipped.</p><p>Tracking: #{TrackingNumber}</p>"
            ),
            ["order.delivered"] = (
                "Order #{OrderNumber} Delivered",
                "<h2>Order Delivered</h2><p>Your order <strong>#{OrderNumber}</strong> has been delivered.</p>"
            ),
            ["order.cancelled"] = (
                "Order #{OrderNumber} Cancelled",
                "<h2>Order Cancelled</h2><p>Your order <strong>#{OrderNumber}</strong> has been cancelled.</p><p>Reason: #{Reason}</p>"
            ),
            ["payment.confirmed"] = (
                "Payment Confirmed",
                "<h2>Payment Confirmed</h2><p>Your payment of ${Amount} has been confirmed.</p>"
            ),
            ["payment.failed"] = (
                "Payment Failed",
                "<h2>Payment Failed</h2><p>Your payment has failed: #{FailureReason}</p>"
            ),
            ["welcome"] = (
                "Welcome to CommerceHub!",
                "<h2>Welcome!</h2><p>Thank you for registering. Start shopping now!</p>"
            ),
            ["password.reset"] = (
                "Reset Your Password",
                "<h2>Password Reset</h2><p>Click <a href='#{ResetLink}'>here</a> to reset your password.</p>"
            )
        };

        if (!templates.TryGetValue(templateName, out var template))
        {
            _logger.LogWarning("Email template {Template} not found", templateName);
            return false;
        }

        var subject = template.Subject;
        var body = template.Body;

        foreach (var (key, value) in templateData)
        {
            subject = subject.Replace($"#{{{key}}}", value);
            body = body.Replace($"#{{{key}}}", value);
        }

        return await SendEmailAsync(to, subject, body, true, cancellationToken);
    }
}
