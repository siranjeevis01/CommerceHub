using MediatR;
using CommerceHub.Modules.Cart.Application.DTOs;

namespace CommerceHub.Modules.Cart.Application.Queries;

public record GetCartQuery : IRequest<CartDto?>
{
    public string CartKey { get; init; } = string.Empty;
}
