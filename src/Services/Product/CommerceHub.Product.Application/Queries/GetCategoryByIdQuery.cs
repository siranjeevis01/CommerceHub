using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDto?>;
