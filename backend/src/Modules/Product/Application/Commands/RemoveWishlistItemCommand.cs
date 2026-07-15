using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record RemoveWishlistItemCommand(int Id) : IRequest;
