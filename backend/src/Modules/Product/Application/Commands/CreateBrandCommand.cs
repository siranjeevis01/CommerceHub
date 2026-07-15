using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record CreateBrandCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
}
