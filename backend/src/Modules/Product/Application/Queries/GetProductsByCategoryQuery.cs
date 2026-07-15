using CommerceHub.Modules.Product.Application.Common.Models;
using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Queries;

public record GetProductsByCategoryQuery(int CategoryId) : PagedRequest, IRequest<ProductSearchResultDto>;
