using MediatR;

namespace CommerceHub.Modules.Analytics.Application.Queries;

public record GetVendorPerformanceQuery : IRequest<VendorPerformanceDto>
{
    public int VendorId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

public record VendorPerformanceDto
{
    public int VendorId { get; init; }
    public int TotalOrders { get; init; }
    public int TotalProductsSold { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalCommission { get; init; }
    public decimal NetEarnings { get; init; }
    public decimal AvgOrderValue { get; init; }
    public DateTime? FirstSale { get; init; }
    public DateTime? LastSale { get; init; }
}
