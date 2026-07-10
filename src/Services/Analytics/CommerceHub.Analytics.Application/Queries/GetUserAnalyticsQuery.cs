using MediatR;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Queries;

public record GetUserAnalyticsQuery : IRequest<UserAnalyticsDto>
{
    public int UserId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

public record UserAnalyticsDto
{
    public int UserId { get; init; }
    public int TotalPageViews { get; init; }
    public int TotalOrders { get; init; }
    public int TotalEvents { get; init; }
    public DateTime? FirstActivity { get; init; }
    public DateTime? LastActivity { get; init; }
    public List<AnalyticsEventDto> RecentEvents { get; init; } = new();
}
