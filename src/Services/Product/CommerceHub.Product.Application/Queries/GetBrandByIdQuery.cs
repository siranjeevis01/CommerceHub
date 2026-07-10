using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetBrandByIdQuery(int Id) : IRequest<BrandDto?>;
