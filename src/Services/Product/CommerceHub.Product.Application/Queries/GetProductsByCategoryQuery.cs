using CommerceHub.Product.Application.Common.Models;
using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetProductsByCategoryQuery(int CategoryId) : PagedRequest, IRequest<ProductSearchResultDto>;
