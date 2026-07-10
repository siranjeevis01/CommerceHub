using MediatR;
using CommerceHub.Cart.Application.DTOs;

namespace CommerceHub.Cart.Application.Queries;

public record GetCartQuery : IRequest<CartDto?>
{
    public string CartKey { get; init; } = string.Empty;
}
