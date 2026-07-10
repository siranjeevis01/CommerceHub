using MediatR;
using CommerceHub.Cms.Application.Common.Interfaces;

namespace CommerceHub.Cms.Application.Commands.Banners;

public record DeleteBannerCommand : IRequest
{
    public int Id { get; init; }
}

public class DeleteBannerCommandHandler : IRequestHandler<DeleteBannerCommand>
{
    private readonly ICmsDbContext _context;

    public DeleteBannerCommandHandler(ICmsDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Banners.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"Banner with Id {request.Id} not found.");

        _context.Banners.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
