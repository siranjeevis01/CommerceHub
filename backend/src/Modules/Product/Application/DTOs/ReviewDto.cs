namespace CommerceHub.Modules.Product.Application.DTOs;

public record ReviewDto
{
    public int Id { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public string? Images { get; init; }
    public bool IsVerifiedPurchase { get; init; }
    public int ProductId { get; init; }
    public int UserId { get; init; }
    public DateTime CreatedAt { get; init; }
}
