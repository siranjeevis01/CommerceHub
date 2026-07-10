using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Application.Common.Interfaces;
using CommerceHub.Order.Application.DTOs;

namespace CommerceHub.Order.Application.Queries;

public record GetOrderByIdQuery : IRequest<OrderDto?>
{
    public int OrderId { get; init; }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        return order is null ? null : _mapper.Map<OrderDto>(order);
    }
}
