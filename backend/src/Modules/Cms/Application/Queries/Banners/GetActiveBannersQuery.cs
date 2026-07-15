using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Application.DTOs;

namespace CommerceHub.Modules.Cms.Application.Queries.Banners;

public record GetActiveBannersQuery : IRequest<List<BannerDto>>;

public class GetActiveBannersQueryHandler : IRequestHandler<GetActiveBannersQuery, List<BannerDto>>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetActiveBannersQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BannerDto>> Handle(GetActiveBannersQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var entities = await _context.Banners
            .AsNoTracking()
            .Where(b => b.IsActive
                     && (!b.StartDate.HasValue || b.StartDate <= now)
                     && (!b.EndDate.HasValue || b.EndDate >= now))
            .OrderBy(b => b.SortOrder)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<BannerDto>>(entities);
    }
}
