using MediatR;
using AutoMapper;
using CommerceHub.Cms.Application.Common.Interfaces;
using CommerceHub.Cms.Application.DTOs;
using CommerceHub.Cms.Domain.Entities;

namespace CommerceHub.Cms.Application.Commands.Pages;

public record UpdatePageCommand : IRequest<PageDto>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Content { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, PageDto>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePageCommandHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PageDto> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CmsPages.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Page with Id {request.Id} not found.");

        entity.Title = request.Title;
        entity.Slug = request.Slug;
        entity.Content = request.Content;
        entity.MetaTitle = request.MetaTitle;
        entity.MetaDescription = request.MetaDescription;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PageDto>(entity);
    }
}
