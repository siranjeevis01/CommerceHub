using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Domain.Entities;

namespace CommerceHub.Modules.Cms.Application.Commands.Coupons;

public record ValidateCouponCommand : IRequest<ValidateCouponResult>
{
    public string Code { get; init; } = string.Empty;
    public decimal OrderTotal { get; init; }
}

public record ValidateCouponResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public CouponDto? Coupon { get; init; }

    public record CouponDto
    {
        public int Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public decimal? DiscountAmount { get; init; }
        public decimal? DiscountPercentage { get; init; }
    }
}

public class ValidateCouponCommandHandler : IRequestHandler<ValidateCouponCommand, ValidateCouponResult>
{
    private readonly ICmsDbContext _context;

    public ValidateCouponCommandHandler(ICmsDbContext context)
    {
        _context = context;
    }

    public async Task<ValidateCouponResult> Handle(ValidateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == request.Code.ToUpperInvariant(), cancellationToken);

        if (coupon is null)
            return new ValidateCouponResult { IsValid = false, ErrorMessage = "Coupon not found." };

        if (!coupon.IsActive)
            return new ValidateCouponResult { IsValid = false, ErrorMessage = "Coupon is not active." };

        if (coupon.ValidFrom.HasValue && coupon.ValidFrom > DateTime.UtcNow)
            return new ValidateCouponResult { IsValid = false, ErrorMessage = "Coupon is not yet valid." };

        if (coupon.ValidTo.HasValue && coupon.ValidTo < DateTime.UtcNow)
            return new ValidateCouponResult { IsValid = false, ErrorMessage = "Coupon has expired." };

        if (coupon.MaxUsageCount.HasValue && coupon.UsedCount >= coupon.MaxUsageCount)
            return new ValidateCouponResult { IsValid = false, ErrorMessage = "Coupon usage limit reached." };

        if (coupon.MinimumOrderAmount.HasValue && request.OrderTotal < coupon.MinimumOrderAmount.Value)
            return new ValidateCouponResult
            {
                IsValid = false,
                ErrorMessage = $"Minimum order amount of {coupon.MinimumOrderAmount.Value:C} required."
            };

        return new ValidateCouponResult
        {
            IsValid = true,
            Coupon = new ValidateCouponResult.CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Type = coupon.Type,
                DiscountAmount = coupon.DiscountAmount,
                DiscountPercentage = coupon.DiscountPercentage
            }
        };
    }
}
