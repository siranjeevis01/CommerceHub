using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Domain.Entities;

namespace CommerceHub.Analytics.Application.Common.Interfaces;

public interface IAnalyticsDbContext
{
    DbSet<AnalyticsEvent> AnalyticsEvents { get; }
    DbSet<DailySummary> DailySummaries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
