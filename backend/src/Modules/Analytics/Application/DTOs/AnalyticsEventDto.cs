namespace CommerceHub.Modules.Analytics.Application.DTOs;

public record AnalyticsEventDto
{
    public int Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? EventData { get; init; }
    public int? UserId { get; init; }
    public string? SessionId { get; init; }
    public string? PageUrl { get; init; }
    public DateTime Timestamp { get; init; }
}
