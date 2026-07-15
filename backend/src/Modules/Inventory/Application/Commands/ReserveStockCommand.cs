using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Inventory.Application.Commands;

public record ReserveStockItem(int ProductId, int? VariantId, int Quantity);
public record ReserveStockResult(bool Success, string? ErrorMessage = null);
public record ReserveStockCommand(int OrderId, List<ReserveStockItem> Items) : IRequest<ReserveStockResult>;

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, ReserveStockResult>
{
    private readonly IInventoryDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public ReserveStockCommandHandler(IInventoryDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ReserveStockResult> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var failedItems = new List<FailedItem>();
        var reservedItems = new List<ReservedItem>();

        foreach (var item in request.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId
                    && i.VariantId == item.VariantId, cancellationToken);

            if (inventory is null || inventory.AvailableQuantity < item.Quantity)
            {
                failedItems.Add(new FailedItem
                {
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    RequestedQuantity = item.Quantity,
                    AvailableQuantity = inventory?.AvailableQuantity ?? 0
                });
                continue;
            }

            inventory.ReservedQuantity += item.Quantity;

            _context.StockMovements.Add(new InventoryTransaction
            {
                InventoryId = inventory.Id,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                QuantityChange = item.Quantity,
                TransactionType = "Reserved",
                Reference = $"Order #{request.OrderId}",
                OrderId = request.OrderId
            });

            reservedItems.Add(new ReservedItem
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                Quantity = item.Quantity
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (failedItems.Count > 0)
        {
            await _publishEndpoint.Publish(new InventoryFailed
            {
                OrderId = request.OrderId,
                Reason = "Insufficient stock for some items",
                FailedItems = failedItems,
                FailedAt = DateTime.UtcNow
            }, cancellationToken);

            return new ReserveStockResult(false, "Insufficient stock for some items");
        }

        await _publishEndpoint.Publish(new InventoryReserved
        {
            OrderId = request.OrderId,
            Items = reservedItems,
            ReservedAt = DateTime.UtcNow
        }, cancellationToken);

        return new ReserveStockResult(true);
    }
}
