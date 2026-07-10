using CommerceHub.Product.Application.Common.Models;
using CommerceHub.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Product.Application.Queries;

public record GetProductsByVendorQuery(int VendorId) : PagedRequest, IRequest<ProductSearchResultDto>;
