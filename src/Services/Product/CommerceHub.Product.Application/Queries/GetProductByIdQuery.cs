using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
