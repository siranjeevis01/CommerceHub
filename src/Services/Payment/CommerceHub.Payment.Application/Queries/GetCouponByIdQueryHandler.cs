using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Payment.Application.Common.Interfaces;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, CouponDto?>
{
    private readonly IPaymentDbContext _context;
    private readonly IMapper _mapper;

    public GetCouponByIdQueryHandler(IPaymentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CouponDto?> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return coupon == null ? null : _mapper.Map<CouponDto>(coupon);
    }
}
