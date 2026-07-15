using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record AddWishlistItemCommand : IRequest<WishlistItemDto>
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
}
