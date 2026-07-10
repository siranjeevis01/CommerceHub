namespace CommerceHub.Analytics.Domain.Entities;

public class DailySummary
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int TotalUsers { get; set; }
    public int NewUsers { get; set; }
    public int TotalOrders { get; set; }
    public int NewOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Revenue { get; set; }
    public int TotalVendors { get; set; }
    public decimal AvgOrderValue { get; set; }
    public double ConversionRate { get; set; }
    public int PageViews { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
