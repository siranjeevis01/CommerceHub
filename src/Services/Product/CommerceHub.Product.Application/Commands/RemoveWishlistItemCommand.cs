using MediatR;

namespace CommerceHub.Product.Application.Commands;

public record RemoveWishlistItemCommand(int Id) : IRequest;
