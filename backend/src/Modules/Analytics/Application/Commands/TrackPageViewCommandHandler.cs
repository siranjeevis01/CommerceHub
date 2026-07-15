using MediatR;
using CommerceHub.Modules.Analytics.Application.Common.Interfaces;
using CommerceHub.Modules.Analytics.Domain.Entities;

namespace CommerceHub.Modules.Analytics.Application.Commands;

public class TrackPageViewCommandHandler : IRequestHandler<TrackPageViewCommand>
{
    private readonly IAnalyticsDbContext _dbContext;

    public TrackPageViewCommandHandler(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(TrackPageViewCommand request, CancellationToken cancellationToken)
    {
        var analyticsEvent = new AnalyticsEvent
        {
            EventType = "PageView",
            EventData = null,
            UserId = request.UserId,
            SessionId = request.SessionId,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            PageUrl = request.PageUrl,
            Timestamp = request.Timestamp
        };

        _dbContext.AnalyticsEvents.Add(analyticsEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
