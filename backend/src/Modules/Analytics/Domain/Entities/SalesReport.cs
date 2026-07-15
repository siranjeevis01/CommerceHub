namespace CommerceHub.Modules.Analytics.Domain.Entities;

public class SalesReport
{
    public int Id { get; set; }
    public DateTime ReportDate { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalEarnings { get; set; }
    public int NewUsers { get; set; }
    public int NewVendors { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
