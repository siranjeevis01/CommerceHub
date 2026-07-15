using MediatR;

namespace CommerceHub.Api.Application.Commands.Admin;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class DashboardStatsDto
{
    public int TotalUsers { get; init; }
    public int TotalVendors { get; init; }
    public int TotalProducts { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public int PendingApprovals { get; init; }
}

public record GetRevenueChartQuery(string? GroupBy = null) : IRequest<List<RevenueChartEntry>>;

public class RevenueChartEntry
{
    public string Period { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int OrderCount { get; init; }
}

public record GetTopProductsQuery(int Top = 10) : IRequest<List<TopProductEntry>>;

public class TopProductEntry
{
    public int ProductId { get; init; }
    public string? ProductName { get; init; }
    public string? MainImageUrl { get; init; }
    public decimal? Price { get; init; }
    public int TotalQuantity { get; init; }
    public decimal TotalRevenue { get; init; }
}
