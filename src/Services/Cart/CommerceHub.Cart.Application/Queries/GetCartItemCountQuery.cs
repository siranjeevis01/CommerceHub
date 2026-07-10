using MediatR;

namespace CommerceHub.Cart.Application.Queries;

public record GetCartItemCountQuery : IRequest<int>
{
    public string CartKey { get; init; } = string.Empty;
}
