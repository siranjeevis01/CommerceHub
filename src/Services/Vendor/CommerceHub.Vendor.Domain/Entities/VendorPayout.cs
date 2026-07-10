namespace CommerceHub.Vendor.Domain.Entities;

public class VendorPayout : BaseEntity
{
    public string PayoutNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public int VendorId { get; set; }
    public VendorProfile Vendor { get; set; } = null!;
}
