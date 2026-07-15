using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.DTOs;
using CommerceHub.Modules.Payment.Domain.Entities;

namespace CommerceHub.Modules.Payment.Application.Commands;

public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, CouponApplyResult>
{
    private readonly IPaymentDbContext _context;

    public ApplyCouponCommandHandler(IPaymentDbContext context)
    {
        _context = context;
    }

    public async Task<CouponApplyResult> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == request.Code && c.IsActive
                && c.StartDate <= DateTime.UtcNow && c.ExpiryDate >= DateTime.UtcNow, cancellationToken);

        if (coupon == null)
            throw new InvalidOperationException("Invalid or expired coupon");

        if (request.OrderAmount < (coupon.MinimumOrderAmount ?? 0))
            throw new InvalidOperationException($"Minimum order amount of {coupon.MinimumOrderAmount} required");

        if (coupon.UsageLimit.HasValue && coupon.CurrentUsageCount >= coupon.UsageLimit.Value)
            throw new InvalidOperationException("Coupon usage limit reached");

        var discount = coupon.CalculateDiscount(request.OrderAmount);

        var usage = new CouponUsage
        {
            CouponId = coupon.Id,
            UserId = request.UserId,
            DiscountAmount = discount,
            OrderAmount = request.OrderAmount,
            UsedAt = DateTime.UtcNow
        };

        _context.CouponUsages.Add(usage);
        coupon.CurrentUsageCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return new CouponApplyResult
        {
            Code = coupon.Code,
            Name = coupon.Name,
            Discount = discount
        };
    }
}
