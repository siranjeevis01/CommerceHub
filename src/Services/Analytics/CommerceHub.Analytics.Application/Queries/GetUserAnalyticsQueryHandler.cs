using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CommerceHub.Analytics.Application.Common.Interfaces;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Queries;

public class GetUserAnalyticsQueryHandler : IRequestHandler<GetUserAnalyticsQuery, UserAnalyticsDto>
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetUserAnalyticsQueryHandler(IAnalyticsDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<UserAnalyticsDto> Handle(GetUserAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AnalyticsEvents
            .Where(e => e.UserId == request.UserId);

        if (request.From.HasValue)
            query = query.Where(e => e.Timestamp >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(e => e.Timestamp <= request.To.Value);

        var events = await query
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        return new UserAnalyticsDto
        {
            UserId = request.UserId,
            TotalPageViews = events.Count(e => e.EventType == "PageView"),
            TotalOrders = events.Count(e => e.EventType == "OrderPlaced"),
            TotalEvents = events.Count,
            FirstActivity = events.MinBy(e => e.Timestamp)?.Timestamp,
            LastActivity = events.MaxBy(e => e.Timestamp)?.Timestamp,
            RecentEvents = _mapper.Map<List<AnalyticsEventDto>>(events.Take(20).ToList())
        };
    }
}
