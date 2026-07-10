using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Application.DTOs;

namespace CommerceHub.Order.Application.Queries;

public record GetOrderStatusQuery : IRequest<OrderStatusDto?>
{
    public int OrderId { get; init; }
}

public class GetOrderStatusQueryHandler : IRequestHandler<GetOrderStatusQuery, OrderStatusDto?>
{
    private readonly IOrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderStatusQueryHandler(IOrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderStatusDto?> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        return order is null ? null : _mapper.Map<OrderStatusDto>(order);
    }
}
