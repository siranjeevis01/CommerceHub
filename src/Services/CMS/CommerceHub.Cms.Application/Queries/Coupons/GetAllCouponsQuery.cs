using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Application.Common.Interfaces;
using CommerceHub.Cms.Application.Common.Models;
using CommerceHub.Cms.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;

namespace CommerceHub.Cms.Application.Queries.Coupons;

public record GetAllCouponsQuery : IRequest<PagedResult<CouponDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public string? CouponType { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, PagedResult<CouponDto>>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetAllCouponsQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<CouponDto>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Coupons.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(c => c.Code.ToLower().Contains(term));
        }

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.CouponType))
            query = query.Where(c => c.Type == request.CouponType);

        var totalCount = await query.CountAsync(cancellationToken);

        query = (request.SortBy?.ToLower()) switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "type" => request.SortDescending ? query.OrderByDescending(c => c.Type) : query.OrderBy(c => c.Type),
            "createdat" => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "validto" => request.SortDescending ? query.OrderByDescending(c => c.ValidTo) : query.OrderBy(c => c.ValidTo),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CouponDto>(
            _mapper.Map<List<CouponDto>>(items),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
