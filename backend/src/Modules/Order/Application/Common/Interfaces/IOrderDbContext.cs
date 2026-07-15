using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Order.Domain.Entities;

namespace CommerceHub.Modules.Order.Application.Common.Interfaces;

public interface IOrderDbContext
{
    DbSet<Domain.Entities.Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderTracking> OrderTrackings { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<ReturnRequest> ReturnRequests { get; }
    DbSet<Dispute> Disputes { get; }
    DbSet<OrderOtp> OrderOtps { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
