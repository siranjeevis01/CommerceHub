using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Payment.Application.Common.Interfaces;
using CommerceHub.Payment.Application.DTOs;

namespace CommerceHub.Payment.Application.Queries;

public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, IReadOnlyList<CouponDto>>
{
    private readonly IPaymentDbContext _context;
    private readonly IMapper _mapper;

    public GetAllCouponsQueryHandler(IPaymentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CouponDto>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
    {
        var coupons = await _context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<CouponDto>>(coupons);
    }
}
