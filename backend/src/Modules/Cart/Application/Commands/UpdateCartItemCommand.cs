using MediatR;
using CommerceHub.Modules.Cart.Application.DTOs;

namespace CommerceHub.Modules.Cart.Application.Commands;

public record UpdateCartItemCommand : IRequest<CartDto>
{
    public string CartKey { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public int Quantity { get; init; }
}
