namespace CommerceHub.Modules.Vendor.Application.DTOs;

public class VendorAnalyticsDto
{
    public int VendorId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal Balance { get; set; }
    public decimal CommissionRate { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int TotalPayouts { get; set; }
}
