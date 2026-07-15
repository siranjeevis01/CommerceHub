using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CommerceHub.Modules.Order.Domain.Sagas;

namespace CommerceHub.Modules.Order.Infrastructure.Data;

public class OrderStateDbContext : SagaDbContext
{
    public OrderStateDbContext(DbContextOptions<OrderStateDbContext> options) : base(options) { }

    public DbSet<OrderState> OrderStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderState>(entity =>
        {
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.OrderNumber).HasMaxLength(50);
            entity.Property(x => x.FailureReason).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();
        });
    }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateMap(); }
    }
}

public class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.OrderNumber).HasMaxLength(50);
        entity.Property(x => x.FailureReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsRowVersion();
    }
}
