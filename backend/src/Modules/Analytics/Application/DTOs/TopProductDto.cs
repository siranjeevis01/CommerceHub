namespace CommerceHub.Modules.Analytics.Application.DTOs;

public record TopProductDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int TotalSold { get; init; }
    public decimal TotalRevenue { get; init; }
}
