using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record UpdateBrandCommand : IRequest
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
}
