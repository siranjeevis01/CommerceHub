using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Application.DTOs;

namespace CommerceHub.Order.Application.Queries;

public record GetOrderByNumberQuery : IRequest<OrderDto?>
{
    public string OrderNumber { get; init; } = string.Empty;
}

public class GetOrderByNumberQueryHandler : IRequestHandler<GetOrderByNumberQuery, OrderDto?>
{
    private readonly IOrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderByNumberQueryHandler(IOrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByNumberQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.OrderNumber == request.OrderNumber, cancellationToken);

        return order is null ? null : _mapper.Map<OrderDto>(order);
    }
}
