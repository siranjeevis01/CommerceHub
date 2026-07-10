using Microsoft.EntityFrameworkCore;
using CommerceHub.Payment.Domain.Entities;

namespace CommerceHub.Payment.Application.Common.Interfaces;

public interface IPaymentDbContext
{
    DbSet<CommerceHub.Payment.Domain.Entities.Payment> Payments { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<CouponUsage> CouponUsages { get; }
    DbSet<GiftCard> GiftCards { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<Refund> Refunds { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
