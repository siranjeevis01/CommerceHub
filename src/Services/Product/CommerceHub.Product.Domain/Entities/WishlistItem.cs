namespace CommerceHub.Product.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
