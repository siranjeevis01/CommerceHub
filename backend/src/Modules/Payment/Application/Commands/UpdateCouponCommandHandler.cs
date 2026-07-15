using MediatR;
using AutoMapper;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Payment.Application.Commands;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, CouponDto>
{
    private readonly IPaymentDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCouponCommandHandler(IPaymentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CouponDto> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Coupon not found");

        coupon.Name = request.Name;
        coupon.DiscountValue = request.DiscountValue;
        coupon.ExpiryDate = request.ExpiryDate;
        coupon.UsageLimit = request.UsageLimit;
        coupon.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CouponDto>(coupon);
    }
}
