using MediatR;

namespace CommerceHub.Modules.Analytics.Application.Commands;

public record TrackPageViewCommand : IRequest
{
    public string PageUrl { get; init; } = string.Empty;
    public string? SessionId { get; init; }
    public int? UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
