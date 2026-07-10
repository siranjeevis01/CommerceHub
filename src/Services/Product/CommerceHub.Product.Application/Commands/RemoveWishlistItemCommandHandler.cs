using CommerceHub.Product.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Commands;

public class RemoveWishlistItemCommandHandler : IRequestHandler<RemoveWishlistItemCommand>
{
    private readonly IProductDbContext _context;

    public RemoveWishlistItemCommandHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveWishlistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (item is null)
            throw new KeyNotFoundException($"Wishlist item with Id {request.Id} was not found.");

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
