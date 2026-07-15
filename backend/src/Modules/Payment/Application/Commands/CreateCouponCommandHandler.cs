using MediatR;
using AutoMapper;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.DTOs;

namespace CommerceHub.Modules.Payment.Application.Commands;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponDto>
{
    private readonly IPaymentDbContext _context;
    private readonly IMapper _mapper;

    public CreateCouponCommandHandler(IPaymentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = new Domain.Entities.Coupon
        {
            Code = request.Code.ToUpperInvariant(),
            Name = request.Name,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MinimumOrderAmount = request.MinimumOrderAmount,
            StartDate = request.StartDate,
            ExpiryDate = request.ExpiryDate,
            UsageLimit = request.UsageLimit,
            PerUserLimit = request.PerUserLimit,
            IsActive = true
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CouponDto>(coupon);
    }
}
