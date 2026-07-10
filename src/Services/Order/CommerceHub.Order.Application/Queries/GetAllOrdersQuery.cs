using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Application.Common.Models;
using CommerceHub.Order.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;

namespace CommerceHub.Order.Application.Queries;

public record GetAllOrdersQuery : PagedRequest, IRequest<PagedResult<OrderListDto>>
{
    public string? OrderStatus { get; init; }
    public string? PaymentStatus { get; init; }
    public string? Search { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderListDto>>
{
    private readonly IOrderDbContext _context;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(IOrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderListDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.OrderStatus))
            query = query.Where(o => o.OrderStatus == request.OrderStatus);

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
            query = query.Where(o => o.PaymentStatus == request.PaymentStatus);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(o =>
                o.OrderNumber.Contains(request.Search) ||
                o.UserId.ToString() == request.Search);

        if (request.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= request.ToDate.Value);

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
