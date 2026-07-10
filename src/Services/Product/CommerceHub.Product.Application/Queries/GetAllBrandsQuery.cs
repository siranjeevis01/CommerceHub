using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetAllBrandsQuery : IRequest<IReadOnlyList<BrandDto>>;
