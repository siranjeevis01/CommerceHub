using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Domain.Entities;
using OrderEntity = CommerceHub.Order.Domain.Entities.Order;

namespace CommerceHub.Order.Infrastructure.Data;

public class OrderDbContext : DbContext, IOrderDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderTracking> OrderTrackings { get; set; } = null!;
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } = null!;
    public DbSet<ReturnRequest> ReturnRequests { get; set; } = null!;
    public DbSet<Dispute> Disputes { get; set; } = null!;
    public DbSet<OrderOtp> OrderOtps { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderEntity>().HasIndex(o => o.OrderNumber).IsUnique();
        modelBuilder.Entity<OrderEntity>().Property(o => o.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<OrderEntity>().Property(o => o.Subtotal).HasPrecision(18, 2);
        modelBuilder.Entity<OrderEntity>().Property(o => o.ShippingCost).HasPrecision(18, 2);
        modelBuilder.Entity<OrderEntity>().Property(o => o.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<OrderEntity>().Property(o => o.DiscountAmount).HasPrecision(18, 2);
        modelBuilder.Entity<OrderEntity>().HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<OrderItem>().Property(i => i.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().Property(i => i.TotalPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().Property(i => i.VendorEarning).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().Property(i => i.Commission).HasPrecision(18, 2);

        modelBuilder.Entity<OrderTracking>().Property(t => t.Latitude).HasPrecision(10, 7);
        modelBuilder.Entity<OrderTracking>().Property(t => t.Longitude).HasPrecision(10, 7);

        modelBuilder.Entity<ReturnRequest>().Property(r => r.RefundAmount).HasPrecision(18, 2);

        modelBuilder.Entity<OrderEntity>().HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId);

        modelBuilder.Entity<OrderEntity>().HasMany(o => o.Trackings)
            .WithOne(t => t.Order)
            .HasForeignKey(t => t.OrderId);

        modelBuilder.Entity<OrderEntity>().HasMany(o => o.StatusHistories)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId);
    }
}
