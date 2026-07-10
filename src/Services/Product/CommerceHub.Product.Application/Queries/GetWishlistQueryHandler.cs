using CommerceHub.Product.Application.Common.Interfaces;
using CommerceHub.Product.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Queries;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, List<WishlistItemDto>>
{
    private readonly IProductDbContext _context;

    public GetWishlistQueryHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<WishlistItemDto>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.WishlistItems
            .AsNoTracking()
            .Include(w => w.Product)
            .Where(w => w.UserId == request.UserId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(w => new WishlistItemDto
        {
            Id = w.Id,
            UserId = w.UserId,
            ProductId = w.ProductId,
            ProductName = w.Product?.Name,
            ProductPrice = w.Product?.Price ?? 0,
            CreatedAt = w.CreatedAt
        }).ToList();
    }
}
