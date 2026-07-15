using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Inventory.Application.Commands;

public record DeductStockItem(int ProductId, int? VariantId, int Quantity);
public record DeductStockResult(bool Success);
public record DeductStockCommand(int OrderId, List<DeductStockItem> Items) : IRequest<DeductStockResult>;

public class DeductStockCommandHandler : IRequestHandler<DeductStockCommand, DeductStockResult>
{
    private readonly IInventoryDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeductStockCommandHandler(IInventoryDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<DeductStockResult> Handle(DeductStockCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId
                    && i.VariantId == item.VariantId, cancellationToken);

            if (inventory is null)
                continue;

            var deductQty = Math.Min(item.Quantity, inventory.StockQuantity);
            inventory.StockQuantity -= deductQty;
            inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - deductQty);

            if (inventory.StockQuantity <= inventory.LowStockThreshold)
            {
                await _publishEndpoint.Publish(new LowStockAlert
                {
                    ProductId = inventory.ProductId,
                    CurrentStock = inventory.StockQuantity,
                    Threshold = inventory.LowStockThreshold
                }, cancellationToken);
            }

            _context.StockMovements.Add(new InventoryTransaction
            {
                InventoryId = inventory.Id,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                QuantityChange = -deductQty,
                TransactionType = "Deducted",
                Reference = $"Order #{request.OrderId} Confirmed",
                OrderId = request.OrderId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new DeductStockResult(true);
    }
}
