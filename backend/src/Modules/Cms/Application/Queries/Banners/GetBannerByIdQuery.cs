using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Application.DTOs;

namespace CommerceHub.Modules.Cms.Application.Queries.Banners;

public record GetBannerByIdQuery : IRequest<BannerDto?>
{
    public int Id { get; init; }
}

public class GetBannerByIdQueryHandler : IRequestHandler<GetBannerByIdQuery, BannerDto?>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetBannerByIdQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BannerDto?> Handle(GetBannerByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Banners
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        return entity is null ? null : _mapper.Map<BannerDto>(entity);
    }
}
