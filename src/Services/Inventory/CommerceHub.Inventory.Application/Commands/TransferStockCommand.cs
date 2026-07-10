using CommerceHub.Inventory.Application.Common.Interfaces;
using CommerceHub.Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Inventory.Application.Commands;

public record TransferStockResult(bool Success, string? ErrorMessage = null);
public record TransferStockCommand(int ProductId, int? VariantId, int FromWarehouseId, int ToWarehouseId, int Quantity) : IRequest<TransferStockResult>;

public class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, TransferStockResult>
{
    private readonly IInventoryDbContext _context;

    public TransferStockCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<TransferStockResult> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        var sourceInventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId
                && i.VariantId == request.VariantId
                && i.WarehouseId == request.FromWarehouseId, cancellationToken);

        if (sourceInventory is null || sourceInventory.StockQuantity < request.Quantity)
            return new TransferStockResult(false, "Insufficient stock in source warehouse");

        var destInventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId
                && i.VariantId == request.VariantId
                && i.WarehouseId == request.ToWarehouseId, cancellationToken);

        if (destInventory is null)
        {
            destInventory = new Domain.Entities.Inventory
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                WarehouseId = request.ToWarehouseId,
                StockQuantity = 0,
                ReservedQuantity = 0,
                LowStockThreshold = 10
            };
            _context.Inventories.Add(destInventory);
        }

        sourceInventory.StockQuantity -= request.Quantity;
        destInventory.StockQuantity += request.Quantity;

        _context.StockMovements.Add(new InventoryTransaction
        {
            InventoryId = sourceInventory.Id,
            ProductId = request.ProductId,
            VariantId = request.VariantId,
            QuantityChange = -request.Quantity,
            TransactionType = "TransferOut",
            Reference = $"Transfer to Warehouse #{request.ToWarehouseId}"
        });

        _context.StockMovements.Add(new InventoryTransaction
        {
            InventoryId = destInventory.Id,
            ProductId = request.ProductId,
            VariantId = request.VariantId,
            QuantityChange = request.Quantity,
            TransactionType = "TransferIn",
            Reference = $"Transfer from Warehouse #{request.FromWarehouseId}"
        });

        await _context.SaveChangesAsync(cancellationToken);
        return new TransferStockResult(true);
    }
}
