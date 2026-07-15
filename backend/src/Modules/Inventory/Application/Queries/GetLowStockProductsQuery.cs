using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Inventory.Application.Queries;

public record GetLowStockProductsQuery(int? Threshold) : IRequest<List<StockLevelDto>>;

public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, List<StockLevelDto>>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetLowStockProductsQueryHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<StockLevelDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Inventories
            .Include(i => i.Warehouse)
            .AsQueryable();

        if (request.Threshold.HasValue)
            query = query.Where(i => i.AvailableQuantity <= request.Threshold.Value);
        else
            query = query.Where(i => i.AvailableQuantity <= i.LowStockThreshold);

        return await query
            .ProjectTo<StockLevelDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
