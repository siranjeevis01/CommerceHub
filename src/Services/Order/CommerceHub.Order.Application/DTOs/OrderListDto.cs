namespace CommerceHub.Order.Application.DTOs;

public record OrderListDto
{
    public int Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int UserId { get; init; }
    public string OrderStatus { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
