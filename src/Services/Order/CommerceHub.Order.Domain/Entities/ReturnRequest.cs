namespace CommerceHub.Order.Domain.Entities;

public class ReturnRequest : BaseEntity
{
    public int OrderId { get; set; }
    public int? OrderItemId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Images { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal? RefundAmount { get; set; }
    public string? RefundMethod { get; set; }
    public Order Order { get; set; } = null!;
}
