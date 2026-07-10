using MediatR;
using CommerceHub.Cart.Application.DTOs;

namespace CommerceHub.Cart.Application.Commands;

public record RemoveFromCartCommand : IRequest<CartDto>
{
    public string CartKey { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
}
