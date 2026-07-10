using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Application.Common.Models;
using CommerceHub.Order.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;

namespace CommerceHub.Order.Application.Queries;

public record GetOrdersByUserQuery : PagedRequest, IRequest<PagedResult<OrderListDto>>
{
    public int UserId { get; init; }
}

public class GetOrdersByUserQueryHandler : IRequestHandler<GetOrdersByUserQuery, PagedResult<OrderListDto>>
{
    private readonly IOrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrdersByUserQueryHandler(IOrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderListDto>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == request.UserId);

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
