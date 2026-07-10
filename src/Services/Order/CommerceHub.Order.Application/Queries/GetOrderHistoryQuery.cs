using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Application.Common.Interfaces;

namespace CommerceHub.Order.Application.Queries;

public record GetOrderHistoryQuery : IRequest<IReadOnlyList<OrderStatusHistoryDto>>
{
    public int OrderId { get; init; }
}

public record OrderStatusHistoryDto
{
    public int Id { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public string? Remarks { get; init; }
    public string? ChangedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class GetOrderHistoryQueryHandler : IRequestHandler<GetOrderHistoryQuery, IReadOnlyList<OrderStatusHistoryDto>>
{
    private readonly IOrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderHistoryQueryHandler(IOrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OrderStatusHistoryDto>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await _context.Orders
            .Where(o => o.Id == request.OrderId)
            .SelectMany(o => o.StatusHistories)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<OrderStatusHistoryDto>>(history);
    }
}
