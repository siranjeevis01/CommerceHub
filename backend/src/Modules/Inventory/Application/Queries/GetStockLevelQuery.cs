using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Inventory.Application.Queries;

public record GetStockLevelQuery(int ProductId, int? VariantId, int WarehouseId) : IRequest<StockLevelDto?>;

public class GetStockLevelQueryHandler : IRequestHandler<GetStockLevelQuery, StockLevelDto?>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetStockLevelQueryHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StockLevelDto?> Handle(GetStockLevelQuery request, CancellationToken cancellationToken)
    {
        return await _context.Inventories
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == request.ProductId
                && i.VariantId == request.VariantId
                && i.WarehouseId == request.WarehouseId)
            .ProjectTo<StockLevelDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
