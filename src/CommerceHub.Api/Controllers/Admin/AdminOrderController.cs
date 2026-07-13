using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.Order.Domain.Entities;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrderController : ControllerBase
{
    private readonly OrderDbContext _orderDb;

    public AdminOrderController(OrderDbContext orderDb)
    {
        _orderDb = orderDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status = null,
        [FromQuery] int? userId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _orderDb.Orders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.OrderStatus == status);

        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);

        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.UserId,
                o.OrderStatus,
                o.PaymentStatus,
                o.TotalAmount,
                o.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = orders });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderDb.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistories)
            .Where(o => o.Id == id)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.UserId,
                o.OrderStatus,
                o.PaymentStatus,
                o.PaymentMethod,
                o.Subtotal,
                o.ShippingCost,
                o.TaxAmount,
                o.DiscountAmount,
                o.TotalAmount,
                o.ShippingAddressId,
                o.CouponId,
                o.CreatedAt,
                items = o.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.ProductVariantId,
                    i.VendorId,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice
                }),
                statusHistory = o.StatusHistories
                    .OrderByDescending(h => h.CreatedAt)
                    .Select(h => new
                    {
                        h.Id,
                        h.FromStatus,
                        h.ToStatus,
                        h.Remarks,
                        h.ChangedBy,
                        h.CreatedAt
                    })
            })
            .FirstOrDefaultAsync();

        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _orderDb.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var fromStatus = order.OrderStatus;
        order.OrderStatus = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        _orderDb.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = id,
            FromStatus = fromStatus,
            ToStatus = request.Status,
            Remarks = request.Remarks,
            ChangedBy = "Admin",
            CreatedAt = DateTime.UtcNow
        });

        await _orderDb.SaveChangesAsync();
        return Ok(new { message = $"Order status updated to {request.Status}" });
    }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
