namespace CommerceHub.Infrastructure.Configurations;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CommerceHub";
    public string Audience { get; set; } = "CommerceHubClient";
    public int ExpiryHours { get; set; } = 24;
}

public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; set; } =
        ["http://localhost:4200", "http://localhost:8100", "http://localhost:3000"];
}

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 2;
}

public sealed class RedisOptions
{
    public const string SectionName = "Redis";
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "CommerceHub_";
}

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    public string Host { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}

public sealed class SeqOptions
{
    public const string SectionName = "Seq";
    public string Url { get; set; } = "http://localhost:5341";
}

public sealed class OtlpOptions
{
    public const string SectionName = "OTLP";
    public string Endpoint { get; set; } = "http://localhost:4317";
}

public sealed class LoggingOptions
{
    public const string SectionName = "Logging";
    public string Directory { get; set; } = "logs";
    public bool FileEnabled { get; set; } = true;
}

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}

public sealed class StripeOptions
{
    public const string SectionName = "Stripe";
    public string PublicKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

public sealed class RazorpayOptions
{
    public const string SectionName = "Razorpay";
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

public sealed class TwilioOptions
{
    public const string SectionName = "Twilio";
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public sealed class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";
    public string PhoneNumberId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string BusinessAccountId { get; set; } = string.Empty;
}

public sealed class StorageOptions
{
    public const string SectionName = "Storage";
    public string Provider { get; set; } = "Local";
    public AwsOptions Aws { get; set; } = new();
    public AzureOptions Azure { get; set; } = new();
}

public sealed class AwsOptions
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string CloudFrontUrl { get; set; } = string.Empty;
}

public sealed class AzureOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}

public sealed class HangfireOptions
{
    public const string SectionName = "Hangfire";
    public bool Enabled { get; set; }
    public string StorageType { get; set; } = "MySQL";
}

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    public string Identity { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Order { get; set; } = string.Empty;
    public string Payment { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Inventory { get; set; } = string.Empty;
    public string Notification { get; set; } = string.Empty;
    public string Cms { get; set; } = string.Empty;
    public string Analytics { get; set; } = string.Empty;
    public string Ai { get; set; } = string.Empty;
}

public sealed class SeedOptions
{
    public const string SectionName = "Seed";
    public bool Enabled { get; set; } = true;
    public string AdminEmail { get; set; } = "admin@commercehub.com";
    public string AdminPassword { get; set; } = "Admin@1234";
}
