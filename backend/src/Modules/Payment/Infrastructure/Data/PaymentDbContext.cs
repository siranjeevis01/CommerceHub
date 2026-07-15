using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Payment.Domain.Entities;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;

namespace CommerceHub.Modules.Payment.Infrastructure.Data;

public class PaymentDbContext : DbContext, IPaymentDbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
    public DbSet<CommerceHub.Modules.Payment.Domain.Entities.Payment> Payments { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Refund> Refunds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();
        modelBuilder.Entity<GiftCard>().HasIndex(g => g.Code).IsUnique();
        modelBuilder.Entity<CommerceHub.Modules.Payment.Domain.Entities.Payment>().Property(p => p.Amount).HasPrecision(18, 2);
    }
}
