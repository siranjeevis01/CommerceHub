namespace CommerceHub.Modules.Vendor.Domain.Entities;

public class VendorProfile : BaseEntity
{
    public string StoreName { get; set; } = string.Empty;
    public string? StoreDescription { get; set; }
    public string? StoreLogo { get; set; }
    public string? StoreBanner { get; set; }
    public string? BusinessPhone { get; set; }
    public string? BusinessEmail { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? BusinessType { get; set; }
    public string? BusinessAddress { get; set; }
    public string VerificationStatus { get; set; } = "Pending";
    public decimal CommissionRate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal Balance { get; set; }
    public int UserId { get; set; }
    public ICollection<VendorDocument> Documents { get; set; } = new List<VendorDocument>();
    public ICollection<VendorPayout> Payouts { get; set; } = new List<VendorPayout>();
}
