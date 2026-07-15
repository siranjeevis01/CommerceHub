namespace CommerceHub.Shared.Contracts.Events;

public record NotificationSent
{
    public int NotificationId { get; init; }
    public int UserId { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool Success { get; init; }
    public DateTime SentAt { get; init; }
}

public record NotificationFailed
{
    public int NotificationId { get; init; }
    public int UserId { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty;
    public int RetryCount { get; init; }
    public DateTime FailedAt { get; init; }
}

public record SendEmailNotification
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsHtml { get; init; } = true;
    public string? TemplateName { get; init; }
    public Dictionary<string, string>? TemplateData { get; init; }
}

public record SendPushNotification
{
    public int UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? LinkUrl { get; init; }
}

public record SendSmsNotification
{
    public string To { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
