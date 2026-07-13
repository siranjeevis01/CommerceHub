using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.Vendor.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Vendor;

[ApiController]
[Route("api/v1/vendor/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;
    private readonly OrderDbContext _orderDb;

    public OrderController(VendorDbContext vendorDb, OrderDbContext orderDb)
    {
        _vendorDb = vendorDb;
        _orderDb = orderDb;
    }

    private int GetUserId() => int.Parse(User.FindFirst("userId")!.Value);

    private async Task<int?> GetVendorIdAsync()
    {
        var userId = GetUserId();
        var vendor = await _vendorDb.Vendors
            .FirstOrDefaultAsync(v => v.UserId == userId && !v.IsDeleted);
        return vendor?.Id;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var query = _orderDb.OrderItems
            .Where(oi => oi.VendorId == vendorId)
            .Include(oi => oi.Order)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(oi => oi.Order.OrderStatus == status);

        if (from.HasValue)
            query = query.Where(oi => oi.Order.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(oi => oi.Order.CreatedAt <= to.Value);

        var totalItems = await query.CountAsync();

        var orders = await query
            .OrderByDescending(oi => oi.Order.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(oi => new
            {
                orderId = oi.Order.Id,
                orderNumber = oi.Order.OrderNumber,
                status = oi.Order.OrderStatus,
                paymentStatus = oi.Order.PaymentStatus,
                customerUserId = oi.Order.UserId,
                itemQuantity = oi.Quantity,
                itemUnitPrice = oi.UnitPrice,
                itemTotalPrice = oi.TotalPrice,
                vendorEarning = oi.VendorEarning,
                commission = oi.Commission,
                orderTotalAmount = oi.Order.TotalAmount,
                orderCreatedAt = oi.Order.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            items = orders,
            totalItems,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var order = await _orderDb.Orders
            .Include(o => o.Items.Where(i => i.VendorId == vendorId))
            .Include(o => o.Trackings)
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound(new { message = "Order not found" });

        var vendorItems = order.Items.Where(i => i.VendorId == vendorId).ToList();
        if (vendorItems.Count == 0) return NotFound(new { message = "No items found for this vendor in this order" });

        return Ok(new
        {
            order.Id,
            order.OrderNumber,
            order.OrderStatus,
            order.PaymentStatus,
            order.PaymentMethod,
            order.UserId,
            order.Subtotal,
            order.ShippingCost,
            order.TaxAmount,
            order.DiscountAmount,
            order.TotalAmount,
            order.CreatedAt,
            vendorItems = vendorItems.Select(i => new
            {
                i.Id,
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.VendorEarning,
                i.Commission
            }),
            trackings = order.Trackings.Select(t => new
            {
                t.Id,
                t.Status,
                t.Description,
                t.LocationName,
                t.Latitude,
                t.Longitude,
                t.CreatedAt
            }),
            statusHistories = order.StatusHistories.Select(h => new
            {
                h.Id,
                fromStatus = h.FromStatus,
                toStatus = h.ToStatus,
                h.Remarks,
                h.ChangedBy,
                h.CreatedAt
            })
        });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var order = await _orderDb.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound(new { message = "Order not found" });

        var hasVendorItems = order.Items.Any(i => i.VendorId == vendorId);
        if (!hasVendorItems) return NotFound(new { message = "No items found for this vendor in this order" });

        var validStatuses = new[] { "Processing", "Shipped", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(request.Status))
            return BadRequest(new { message = $"Invalid status. Valid statuses: {string.Join(", ", validStatuses)}" });

        var oldStatus = order.OrderStatus;
        order.OrderStatus = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        _orderDb.OrderStatusHistories.Add(new CommerceHub.Order.Domain.Entities.OrderStatusHistory
        {
            OrderId = id,
            FromStatus = oldStatus,
            ToStatus = request.Status
        });

        await _orderDb.SaveChangesAsync();

        return Ok(new { message = "Order status updated", oldStatus, newStatus = request.Status });
    }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
