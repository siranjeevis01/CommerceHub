using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Inventory.Application.Common.Interfaces;
using CommerceHub.Inventory.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Inventory.Application.Queries;

public record GetStockByProductQuery(int ProductId, int? VariantId) : IRequest<List<StockLevelDto>>;

public class GetStockByProductQueryHandler : IRequestHandler<GetStockByProductQuery, List<StockLevelDto>>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetStockByProductQueryHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<StockLevelDto>> Handle(GetStockByProductQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Inventories
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == request.ProductId);

        if (request.VariantId.HasValue)
            query = query.Where(i => i.VariantId == request.VariantId);

        return await query
            .ProjectTo<StockLevelDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
