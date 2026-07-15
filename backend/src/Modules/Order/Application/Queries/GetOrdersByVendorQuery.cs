using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Order.Application.Common.Interfaces;
using CommerceHub.Modules.Order.Application.Common.Models;
using CommerceHub.Modules.Order.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;

namespace CommerceHub.Modules.Order.Application.Queries;

public record GetOrdersByVendorQuery : PagedRequest, IRequest<PagedResult<OrderListDto>>
{
    public int VendorId { get; init; }
}

public class GetOrdersByVendorQueryHandler : IRequestHandler<GetOrdersByVendorQuery, PagedResult<OrderListDto>>
{
    private readonly IOrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrdersByVendorQueryHandler(IOrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderListDto>> Handle(GetOrdersByVendorQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.Items.Any(i => i.VendorId == request.VendorId));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<OrderListDto>>(items);

        return new PagedResult<OrderListDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
