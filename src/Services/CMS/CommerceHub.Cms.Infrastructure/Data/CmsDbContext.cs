using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Application.Common.Interfaces;
using CommerceHub.Cms.Domain.Entities;

namespace CommerceHub.Cms.Infrastructure.Data;

public class CmsDbContext : DbContext, ICmsDbContext
{
    public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options) { }

    public DbSet<Menu> Menus { get; set; }
    public DbSet<RoleMenu> RoleMenus { get; set; }
    public DbSet<PlatformSetting> PlatformSettings { get; set; }
    public DbSet<FeatureToggle> FeatureToggles { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<CmsPage> CmsPages { get; set; }
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Menu>()
            .HasOne(m => m.ParentMenu)
            .WithMany(m => m.SubMenus)
            .HasForeignKey(m => m.ParentMenuId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.LinkUrl).HasMaxLength(1000);
        });

        modelBuilder.Entity<CmsPage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.HasIndex(e => e.Slug);
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.HasIndex(e => e.Code);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<FeatureToggle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(100);
            entity.HasIndex(e => e.Key);
        });

        modelBuilder.Entity<PlatformSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(100);
            entity.HasIndex(e => e.Key);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<RoleMenu>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
