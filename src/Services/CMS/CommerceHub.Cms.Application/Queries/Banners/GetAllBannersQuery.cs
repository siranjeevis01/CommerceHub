using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Application.Common.Interfaces;
using CommerceHub.Cms.Application.Common.Models;
using CommerceHub.Cms.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;

namespace CommerceHub.Cms.Application.Queries.Banners;

public record GetAllBannersQuery : IRequest<PagedResult<BannerDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

public class GetAllBannersQueryHandler : IRequestHandler<GetAllBannersQuery, PagedResult<BannerDto>>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetAllBannersQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<BannerDto>> Handle(GetAllBannersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Banners.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(term));
        }

        if (request.IsActive.HasValue)
            query = query.Where(b => b.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = (request.SortBy?.ToLower()) switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "sortorder" => request.SortDescending ? query.OrderByDescending(b => b.SortOrder) : query.OrderBy(b => b.SortOrder),
            "createdat" => request.SortDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
            _ => query.OrderBy(b => b.SortOrder)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BannerDto>(
            _mapper.Map<List<BannerDto>>(items),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
