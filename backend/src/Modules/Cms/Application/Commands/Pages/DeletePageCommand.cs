using MediatR;
using CommerceHub.Modules.Cms.Application.Common.Interfaces;

namespace CommerceHub.Modules.Cms.Application.Commands.Pages;

public record DeletePageCommand : IRequest
{
    public int Id { get; init; }
}

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand>
{
    private readonly ICmsDbContext _context;

    public DeletePageCommandHandler(ICmsDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CmsPages.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Page with Id {request.Id} not found.");

        _context.CmsPages.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
