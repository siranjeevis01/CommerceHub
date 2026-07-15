using MediatR;

namespace CommerceHub.Modules.Analytics.Application.Commands;

public record TrackEventCommand : IRequest
{
    public string EventType { get; init; } = string.Empty;
    public string? EventData { get; init; }
    public int? UserId { get; init; }
    public string? SessionId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
