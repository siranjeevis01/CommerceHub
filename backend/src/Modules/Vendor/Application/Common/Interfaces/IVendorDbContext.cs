using CommerceHub.Modules.Vendor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Vendor.Application.Common.Interfaces;

public interface IVendorDbContext
{
    DbSet<VendorProfile> Vendors { get; }
    DbSet<VendorDocument> VendorDocuments { get; }
    DbSet<VendorPayout> Payouts { get; }
    DbSet<CommissionConfig> Commissions { get; }
    DbSet<Settlement> Settlements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
