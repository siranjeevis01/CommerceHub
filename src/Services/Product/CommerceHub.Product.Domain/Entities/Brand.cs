namespace CommerceHub.Product.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
