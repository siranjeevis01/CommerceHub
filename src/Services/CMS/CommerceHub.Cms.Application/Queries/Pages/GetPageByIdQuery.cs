using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Application.Common.Interfaces;
using CommerceHub.Cms.Application.DTOs;

namespace CommerceHub.Cms.Application.Queries.Pages;

public record GetPageByIdQuery : IRequest<PageDto?>
{
    public int Id { get; init; }
}

public class GetPageByIdQueryHandler : IRequestHandler<GetPageByIdQuery, PageDto?>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public GetPageByIdQueryHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PageDto?> Handle(GetPageByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.CmsPages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return entity is null ? null : _mapper.Map<PageDto>(entity);
    }
}
