using MediatR;
using AutoMapper;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Application.DTOs;
using CommerceHub.Modules.Cms.Domain.Entities;

namespace CommerceHub.Modules.Cms.Application.Commands.Coupons;

public record CreateCouponCommand : IRequest<int>
{
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal? DiscountAmount { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public decimal? MinimumOrderAmount { get; init; }
    public int? MaxUsageCount { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
}

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, int>
{
    private readonly ICmsDbContext _context;

    public CreateCouponCommandHandler(ICmsDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = new Coupon
        {
            Code = request.Code.ToUpperInvariant(),
            Type = request.Type,
            DiscountAmount = request.DiscountAmount,
            DiscountPercentage = request.DiscountPercentage,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaxUsageCount = request.MaxUsageCount,
            UsedCount = 0,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Coupons.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
