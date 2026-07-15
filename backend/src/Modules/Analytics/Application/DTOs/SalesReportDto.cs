namespace CommerceHub.Modules.Analytics.Application.DTOs;

public record SalesReportDto
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public decimal TotalSales { get; init; }
    public int TotalOrders { get; init; }
    public decimal AvgOrderValue { get; init; }
    public List<DailySalesDto> DailySales { get; init; } = new();
}

public record DailySalesDto
{
    public DateTime Date { get; init; }
    public int OrderCount { get; init; }
    public decimal Revenue { get; init; }
}
