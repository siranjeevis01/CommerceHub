using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Vendor.Domain.Entities;
using CommerceHub.Modules.Vendor.Application.Common.Interfaces;

namespace CommerceHub.Modules.Vendor.Infrastructure.Data;

public class VendorDbContext : DbContext, IVendorDbContext
{
    public VendorDbContext(DbContextOptions<VendorDbContext> options) : base(options) { }
    public DbSet<VendorProfile> Vendors { get; set; }
    public DbSet<VendorDocument> VendorDocuments { get; set; }
    public DbSet<VendorPayout> Payouts { get; set; }
    public DbSet<Settlement> Settlements { get; set; }
    public DbSet<CommissionConfig> Commissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<VendorPayout>().Property(p => p.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Settlement>().Property(s => s.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<VendorProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VendorProfile>().ToTable("VendorProfiles");
        modelBuilder.Entity<VendorPayout>().ToTable("VendorPayouts");
        modelBuilder.Entity<CommissionConfig>().ToTable("CommissionConfigs");
    }
}
