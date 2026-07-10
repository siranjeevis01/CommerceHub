namespace CommerceHub.Vendor.Domain.Entities;

public class Settlement : BaseEntity
{
    public int VendorId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalEarnings { get; set; }
    public string Status { get; set; } = "Pending";
}
