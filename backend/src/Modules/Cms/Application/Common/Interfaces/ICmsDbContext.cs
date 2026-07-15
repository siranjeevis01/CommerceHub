using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Cms.Domain.Entities;

namespace CommerceHub.Modules.Cms.Application.Common.Interfaces;

public interface ICmsDbContext
{
    DbSet<CmsPage> CmsPages { get; }
    DbSet<Banner> Banners { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Campaign> Campaigns { get; }
    DbSet<FeatureToggle> FeatureToggles { get; }
    DbSet<PlatformSetting> PlatformSettings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
