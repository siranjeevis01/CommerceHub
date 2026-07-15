using MediatR;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;

namespace CommerceHub.Modules.Cms.Application.Commands.Pages;

public record PublishPageCommand : IRequest
{
    public int Id { get; init; }
}

public class PublishPageCommandHandler : IRequestHandler<PublishPageCommand>
{
    private readonly ICmsDbContext _context;

    public PublishPageCommandHandler(ICmsDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PublishPageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CmsPages.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Page with Id {request.Id} not found.");

        entity.IsPublished = true;
        entity.PublishedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
