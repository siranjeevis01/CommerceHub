using MediatR;
using AutoMapper;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Application.DTOs;
using CommerceHub.Modules.Cms.Domain.Entities;

namespace CommerceHub.Modules.Cms.Application.Commands.Banners;

public record UpdateBannerCommand : IRequest<BannerDto>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? LinkUrl { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, BannerDto>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public UpdateBannerCommandHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BannerDto> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Banners.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Banner with Id {request.Id} not found.");

        entity.Title = request.Title;
        entity.Subtitle = request.Subtitle;
        entity.ImageUrl = request.ImageUrl;
        entity.LinkUrl = request.LinkUrl;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BannerDto>(entity);
    }
}
