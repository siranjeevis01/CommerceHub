using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Application.Common.Interfaces;
using CommerceHub.Cms.Application.DTOs;

namespace CommerceHub.Cms.Application.Queries.Coupons;

public record GetCouponByIdQuery : IRequest<CouponDto?>
{
    public int Id { get; init; }
}

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, CouponDto?>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetCouponByIdQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CouponDto?> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return entity is null ? null : _mapper.Map<CouponDto>(entity);
    }
}
