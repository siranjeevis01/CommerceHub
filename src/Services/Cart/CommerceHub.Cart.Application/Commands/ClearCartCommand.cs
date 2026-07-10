using MediatR;

namespace CommerceHub.Cart.Application.Commands;

public record ClearCartCommand : IRequest
{
    public string CartKey { get; init; } = string.Empty;
}
