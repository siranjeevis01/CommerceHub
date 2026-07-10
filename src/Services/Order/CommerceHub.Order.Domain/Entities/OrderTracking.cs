namespace CommerceHub.Order.Domain.Entities;

public class OrderTracking : BaseEntity
{
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LocationName { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
