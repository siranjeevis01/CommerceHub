using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Application.DTOs;
using CommerceHub.Shared.Kernel.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Inventory.Application.Queries;

public record GetStockMovementsQuery(
    int Page = 1,
    int PageSize = 10,
    int? ProductId = null,
    int? WarehouseId = null,
    string? TransactionType = null) : IRequest<PagedResult<StockMovementDto>>;

public class GetStockMovementsQueryHandler : IRequestHandler<GetStockMovementsQuery, PagedResult<StockMovementDto>>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetStockMovementsQueryHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<StockMovementDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StockMovements.AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(sm => sm.ProductId == request.ProductId.Value);

        if (request.WarehouseId.HasValue)
            query = query.Where(sm => sm.InventoryId == request.WarehouseId.Value);

        if (!string.IsNullOrWhiteSpace(request.TransactionType))
            query = query.Where(sm => sm.TransactionType == request.TransactionType);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<StockMovementDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<StockMovementDto>(items, totalCount, request.Page, request.PageSize);
    }
}
