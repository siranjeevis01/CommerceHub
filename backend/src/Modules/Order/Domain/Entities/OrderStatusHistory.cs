namespace CommerceHub.Modules.Order.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public int OrderId { get; set; }
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? OtpCode { get; set; }
    public string? ChangedBy { get; set; }
    public Order Order { get; set; } = null!;
}
