using CommerceHub.Modules.Product.Application.Common.Models;
using CommerceHub.Modules.Product.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Product.Application.Queries;

public record GetProductsByVendorQuery(int VendorId) : PagedRequest, IRequest<ProductSearchResultDto>;
