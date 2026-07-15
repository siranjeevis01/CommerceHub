using MediatR;
using AutoMapper;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;
using CommerceHub.Modules.Cms.Application.DTOs;
using CommerceHub.Modules.Cms.Domain.Entities;

namespace CommerceHub.Modules.Cms.Application.Commands.Pages;

public record CreatePageCommand : IRequest<int>
{
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Content { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, int>
{
    private readonly ICmsDbContext _context;
    private readonly IMapper _mapper;

    public CreatePageCommandHandler(ICmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        var entity = new CmsPage
        {
            Title = request.Title,
            Slug = request.Slug,
            Content = request.Content,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.CmsPages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
