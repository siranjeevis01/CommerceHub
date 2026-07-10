using AutoMapper;
using CommerceHub.Inventory.Application.Common.Interfaces;
using CommerceHub.Inventory.Application.DTOs;
using CommerceHub.Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Inventory.Application.Commands;

public record AddStockCommand(int ProductId, int? VariantId, int WarehouseId, int Quantity) : IRequest<StockLevelDto>;

public class AddStockCommandHandler : IRequestHandler<AddStockCommand, StockLevelDto>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public AddStockCommandHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StockLevelDto> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId
                && i.VariantId == request.VariantId
                && i.WarehouseId == request.WarehouseId, cancellationToken);

        if (inventory is null)
        {
            inventory = new Domain.Entities.Inventory
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                WarehouseId = request.WarehouseId,
                StockQuantity = 0,
                ReservedQuantity = 0,
                LowStockThreshold = 10
            };
            _context.Inventories.Add(inventory);
        }

        inventory.StockQuantity += request.Quantity;

        _context.StockMovements.Add(new InventoryTransaction
        {
            InventoryId = inventory.Id,
            ProductId = request.ProductId,
            VariantId = request.VariantId,
            QuantityChange = request.Quantity,
            TransactionType = "StockIn",
            Reference = "Manual addition"
        });

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StockLevelDto>(inventory);
    }
}
