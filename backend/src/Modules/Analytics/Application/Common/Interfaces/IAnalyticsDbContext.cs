using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Analytics.Domain.Entities;

namespace CommerceHub.Modules.Analytics.Application.Common.Interfaces;

public interface IAnalyticsDbContext
{
    DbSet<AnalyticsEvent> AnalyticsEvents { get; }
    DbSet<DailySummary> DailySummaries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
