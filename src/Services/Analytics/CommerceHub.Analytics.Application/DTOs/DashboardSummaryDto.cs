namespace CommerceHub.Analytics.Application.DTOs;

public record DashboardSummaryDto
{
    public int TotalUsers { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public int TotalVendors { get; init; }
    public decimal AvgOrderValue { get; init; }
    public double ConversionRate { get; init; }
    public int NewUsersToday { get; init; }
    public int OrdersToday { get; init; }
    public decimal RevenueToday { get; init; }
}
