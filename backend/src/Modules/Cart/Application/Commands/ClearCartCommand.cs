using MediatR;

namespace CommerceHub.Modules.Cart.Application.Commands;

public record ClearCartCommand : IRequest
{
    public string CartKey { get; init; } = string.Empty;
}
