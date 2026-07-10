using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Application.Common.Interfaces;
using CommerceHub.Cms.Application.Common.Models;
using CommerceHub.Cms.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;

namespace CommerceHub.Cms.Application.Queries.Pages;

public record GetAllPagesQuery : IRequest<PagedResult<PageDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public bool? IsPublished { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

public class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, PagedResult<PageDto>>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetAllPagesQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<PageDto>> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CmsPages.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(term)
                                  || p.Slug.ToLower().Contains(term));
        }

        if (request.IsPublished.HasValue)
            query = query.Where(p => p.IsPublished == request.IsPublished.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = (request.SortBy?.ToLower()) switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "createdat" => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "publishedat" => request.SortDescending ? query.OrderByDescending(p => p.PublishedAt) : query.OrderBy(p => p.PublishedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PageDto>(
            _mapper.Map<List<PageDto>>(items),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
