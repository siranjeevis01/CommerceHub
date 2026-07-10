using MediatR;
using CommerceHub.Analytics.Application.Common.Interfaces;
using CommerceHub.Analytics.Domain.Entities;

namespace CommerceHub.Analytics.Application.Commands;

public class TrackEventCommandHandler : IRequestHandler<TrackEventCommand>
{
    private readonly IAnalyticsDbContext _dbContext;

    public TrackEventCommandHandler(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(TrackEventCommand request, CancellationToken cancellationToken)
    {
        var analyticsEvent = new AnalyticsEvent
        {
            EventType = request.EventType,
            EventData = request.EventData,
            UserId = request.UserId,
            SessionId = request.SessionId,
            Timestamp = request.Timestamp
        };

        _dbContext.AnalyticsEvents.Add(analyticsEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
