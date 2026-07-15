using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Queries;

public record GetBrandByIdQuery(int Id) : IRequest<BrandDto?>;
