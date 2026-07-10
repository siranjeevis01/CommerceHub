using CommerceHub.Vendor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Vendor.Application.Common.Interfaces;

public interface IVendorDbContext
{
    DbSet<VendorProfile> Vendors { get; }
    DbSet<VendorPayout> Payouts { get; }
    DbSet<CommissionConfig> Commissions { get; }
    DbSet<Settlement> Settlements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
