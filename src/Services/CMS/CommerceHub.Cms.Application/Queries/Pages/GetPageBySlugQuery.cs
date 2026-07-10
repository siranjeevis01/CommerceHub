using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Application.Common.Interfaces;
using CommerceHub.Cms.Application.DTOs;

namespace CommerceHub.Cms.Application.Queries.Pages;

public record GetPageBySlugQuery : IRequest<PageDto?>
{
    public string Slug { get; init; } = string.Empty;
}

public class GetPageBySlugQueryHandler : IRequestHandler<GetPageBySlugQuery, PageDto?>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetPageBySlugQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PageDto?> Handle(GetPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.CmsPages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == request.Slug, cancellationToken);

        return entity is null ? null : _mapper.Map<PageDto>(entity);
    }
}
