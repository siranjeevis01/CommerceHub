namespace CommerceHub.Product.Application.DTOs;

public record WishlistItemDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public int ProductId { get; init; }
    public string? ProductName { get; init; }
    public decimal ProductPrice { get; init; }
    public DateTime CreatedAt { get; init; }
}
