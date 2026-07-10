namespace CommerceHub.Analytics.Domain.Entities;

public class AnalyticsEvent
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? PageUrl { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
