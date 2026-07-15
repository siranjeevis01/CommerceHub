namespace CommerceHub.Modules.Vendor.Domain.Entities;

public class VendorDocument : BaseEntity
{
    public int VendorId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? VerificationStatus { get; set; }
    public VendorProfile Vendor { get; set; } = null!;
}
