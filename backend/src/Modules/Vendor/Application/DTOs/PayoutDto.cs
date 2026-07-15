namespace CommerceHub.Modules.Vendor.Application.DTOs;

public class PayoutDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string PayoutNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
