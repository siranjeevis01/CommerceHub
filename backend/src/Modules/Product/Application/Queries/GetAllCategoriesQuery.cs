using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Queries;

public record GetAllCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
