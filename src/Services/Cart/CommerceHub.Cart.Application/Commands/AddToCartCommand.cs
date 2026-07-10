using MediatR;
using CommerceHub.Cart.Application.DTOs;

namespace CommerceHub.Cart.Application.Commands;

public record AddToCartCommand : IRequest<CartDto>
{
    public string CartKey { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; } = 1;
}
