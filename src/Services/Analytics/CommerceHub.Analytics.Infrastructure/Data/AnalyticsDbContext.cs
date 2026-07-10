using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Application.Common.Interfaces;
using CommerceHub.Analytics.Domain.Entities;

namespace CommerceHub.Analytics.Infrastructure.Data;

public class AnalyticsDbContext : DbContext, IAnalyticsDbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<SalesReport> SalesReports { get; set; } = null!;
    public DbSet<DailySummary> DailySummaries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalyticsEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.EventData).HasMaxLength(5000);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.PageUrl).HasMaxLength(2000);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Entity).HasMaxLength(100);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Entity);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<SalesReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Period).HasMaxLength(50);
            entity.HasIndex(e => e.ReportDate);
        });

        modelBuilder.Entity<DailySummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date);
        });
    }
}
