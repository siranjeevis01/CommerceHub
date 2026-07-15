using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Application.DTOs;

namespace CommerceHub.Modules.Cms.Application.Queries.Coupons;

public record GetCouponByCodeQuery : IRequest<CouponDto?>
{
    public string Code { get; init; } = string.Empty;
}

public class GetCouponByCodeQueryHandler : IRequestHandler<GetCouponByCodeQuery, CouponDto?>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetCouponByCodeQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CouponDto?> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == request.Code.ToUpperInvariant(), cancellationToken);

        return entity is null ? null : _mapper.Map<CouponDto>(entity);
    }
}
