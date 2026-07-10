namespace CommerceHub.Order.Domain.Entities;

public class Dispute : BaseEntity
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public Order Order { get; set; } = null!;
}
