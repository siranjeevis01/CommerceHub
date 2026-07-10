using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetWishlistQuery(int UserId) : IRequest<List<WishlistItemDto>>;
