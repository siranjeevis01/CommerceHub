using CommerceHub.Inventory.Application.Common.Interfaces;
using CommerceHub.Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Inventory.Application.Commands;

public record ReleaseStockItem(int ProductId, int? VariantId, int Quantity);
public record ReleaseStockResult(bool Success);
public record ReleaseStockCommand(int OrderId, List<ReleaseStockItem> Items) : IRequest<ReleaseStockResult>;

public class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand, ReleaseStockResult>
{
    private readonly IInventoryDbContext _context;

    public ReleaseStockCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<ReleaseStockResult> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId
                    && i.VariantId == item.VariantId, cancellationToken);

            if (inventory is null)
                continue;

            var actualRelease = Math.Min(item.Quantity, inventory.ReservedQuantity);
            inventory.ReservedQuantity -= actualRelease;

            _context.StockMovements.Add(new InventoryTransaction
            {
                InventoryId = inventory.Id,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                QuantityChange = -actualRelease,
                TransactionType = "Released",
                Reference = $"Order #{request.OrderId} Release",
                OrderId = request.OrderId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new ReleaseStockResult(true);
    }
}
