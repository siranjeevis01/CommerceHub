using System.Net;
using System.Net.Mail;
using System.Reflection;
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
    private readonly Dictionary<string, (string Subject, string Body)> _templates = new();

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _smtpHost = configuration["Smtp:Host"] ?? Environment.GetEnvironmentVariable("SMTP_HOST") ?? "localhost";
        _smtpPort = int.Parse(configuration["Smtp:Port"] ?? Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _smtpUser = configuration["Smtp:User"] ?? Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
        _smtpPass = configuration["Smtp:Pass"] ?? Environment.GetEnvironmentVariable("SMTP_PASS") ?? "";
        _fromEmail = configuration["Smtp:FromEmail"] ?? Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "noreply@commercehub.com";
        _fromName = configuration["Smtp:FromName"] ?? Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "CommerceHub";
        _logger = logger;
        LoadTemplates();
    }

    private void LoadTemplates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var templateNames = new Dictionary<string, string>
        {
            ["order.confirmed"] = "order.confirmed",
            ["order.shipped"] = "order.shipped",
            ["order.delivered"] = "order.delivered",
            ["order.cancelled"] = "order.cancelled",
            ["payment.confirmed"] = "payment.confirmed",
            ["payment.failed"] = "payment.failed",
            ["welcome"] = "welcome",
            ["password.reset"] = "password.reset"
        };

        foreach (var (templateName, resourceName) in templateNames)
        {
            try
            {
                var fullName = $"CommerceHub.Notification.Infrastructure.Templates.{resourceName}.html";
                using var stream = assembly.GetManifestResourceStream(fullName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    var body = reader.ReadToEnd();
                    var subject = templateName switch
                    {
                        "order.confirmed" => "Order #{OrderNumber} Confirmed",
                        "order.shipped" => "Order #{OrderNumber} Shipped",
                        "order.delivered" => "Order #{OrderNumber} Delivered",
                        "order.cancelled" => "Order #{OrderNumber} Cancelled",
                        "payment.confirmed" => "Payment Confirmed - Order #{OrderNumber}",
                        "payment.failed" => "Payment Failed - Order #{OrderNumber}",
                        "welcome" => "Welcome to CommerceHub!",
                        "password.reset" => "Reset Your Password",
                        _ => templateName
                    };
                    _templates[templateName] = (subject, body);
                    _logger.LogInformation("Loaded email template: {Template}", templateName);
                }
                else
                {
                    _logger.LogWarning("Email template resource not found: {FullName}", fullName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load email template: {Template}", templateName);
            }
        }

        _logger.LogInformation("Loaded {Count} email templates from embedded resources", _templates.Count);
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
        if (!_templates.TryGetValue(templateName, out var template))
        {
            _logger.LogWarning("Email template {Template} not found", templateName);
            return false;
        }

        var subject = template.Subject;
        var body = template.Body;

        foreach (var (key, value) in templateData)
        {
            subject = subject.Replace($"#{{{key}}}", value ?? string.Empty);
            body = body.Replace($"#{{{key}}}", value ?? string.Empty);
        }

        return await SendEmailAsync(to, subject, body, true, cancellationToken);
    }
}
