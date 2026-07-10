using CommerceHub.Product.Application.Common.Interfaces;
using CommerceHub.Product.Application.DTOs;
using CommerceHub.Product.Domain.Entities;
using MediatR;

namespace CommerceHub.Product.Application.Commands;

public class AddWishlistItemCommandHandler : IRequestHandler<AddWishlistItemCommand, WishlistItemDto>
{
    private readonly IProductDbContext _context;

    public AddWishlistItemCommandHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task<WishlistItemDto> Handle(AddWishlistItemCommand request, CancellationToken cancellationToken)
    {
        var existing = _context.WishlistItems
            .FirstOrDefault(w => w.UserId == request.UserId && w.ProductId == request.ProductId);

        if (existing is not null)
            throw new InvalidOperationException("Item already in wishlist.");

        var item = new WishlistItem
        {
            UserId = request.UserId,
            ProductId = request.ProductId
        };

        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);

        return new WishlistItemDto
        {
            Id = item.Id,
            UserId = item.UserId,
            ProductId = item.ProductId,
            ProductName = product?.Name,
            ProductPrice = product?.Price ?? 0,
            CreatedAt = item.CreatedAt
        };
    }
}
