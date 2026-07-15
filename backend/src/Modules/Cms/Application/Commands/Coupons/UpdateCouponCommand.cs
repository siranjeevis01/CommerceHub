using MediatR;
using AutoMapper;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Application.DTOs;
using CommerceHub.Modules.Cms.Domain.Entities;

namespace CommerceHub.Modules.Cms.Application.Commands.Coupons;

public record UpdateCouponCommand : IRequest<CouponDto>
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal? DiscountAmount { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public decimal? MinimumOrderAmount { get; init; }
    public int? MaxUsageCount { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, CouponDto>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCouponCommandHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CouponDto> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Coupons.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Coupon with Id {request.Id} not found.");

        entity.Code = request.Code.ToUpperInvariant();
        entity.Type = request.Type;
        entity.DiscountAmount = request.DiscountAmount;
        entity.DiscountPercentage = request.DiscountPercentage;
        entity.MinimumOrderAmount = request.MinimumOrderAmount;
        entity.MaxUsageCount = request.MaxUsageCount;
        entity.ValidFrom = request.ValidFrom;
        entity.ValidTo = request.ValidTo;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CouponDto>(entity);
    }
}
