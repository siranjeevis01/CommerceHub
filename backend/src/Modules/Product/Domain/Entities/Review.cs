namespace CommerceHub.Modules.Product.Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? Images { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public Product Product { get; set; } = null!;
}
