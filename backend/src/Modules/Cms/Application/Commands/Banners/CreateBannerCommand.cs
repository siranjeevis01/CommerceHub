using MediatR;
using AutoMapper;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Application.DTOs;
using CommerceHub.Modules.Cms.Domain.Entities;

namespace CommerceHub.Modules.Cms.Application.Commands.Banners;

public record CreateBannerCommand : IRequest<int>
{
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? LinkUrl { get; init; }
    public int SortOrder { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, int>
{
    private readonly ICmsDbContext _context;

    public CreateBannerCommandHandler(ICmsDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        var entity = new Banner
        {
            Title = request.Title,
            Subtitle = request.Subtitle,
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            SortOrder = request.SortOrder,
            IsActive = true,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Banners.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
