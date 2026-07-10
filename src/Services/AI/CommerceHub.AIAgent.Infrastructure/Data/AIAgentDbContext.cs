using Microsoft.EntityFrameworkCore;
using CommerceHub.AIAgent.Domain.Entities;
using CommerceHub.AIAgent.Application.Common.Interfaces;

namespace CommerceHub.AIAgent.Infrastructure.Data;

public class AIAgentDbContext : DbContext, IAIAgentDbContext
{
    public AIAgentDbContext(DbContextOptions<AIAgentDbContext> options) : base(options) { }

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ProductRecommendation> ProductRecommendations { get; set; }
    public DbSet<SearchQuery> SearchQueries { get; set; }
    public DbSet<AISession> AISessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("AIAgent_Conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasMany(e => e.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("AIAgent_Messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Intent).HasMaxLength(200);
            entity.Property(e => e.Confidence).HasMaxLength(50);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ProductRecommendation>(entity =>
        {
            entity.ToTable("AIAgent_ProductRecommendations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RecommendationType).HasMaxLength(100);
            entity.Property(e => e.ProductName).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.HasIndex(e => new { e.UserId, e.RecommendationType });
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<SearchQuery>(entity =>
        {
            entity.ToTable("AIAgent_SearchQueries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ParsedIntent).HasMaxLength(200);
            entity.HasIndex(e => e.UserId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AISession>(entity =>
        {
            entity.ToTable("AIAgent_Sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionToken).HasMaxLength(500);
            entity.HasIndex(e => e.SessionToken).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}