using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Queries;

public record GetAllBrandsQuery : IRequest<IReadOnlyList<BrandDto>>;
