namespace CommerceHub.Product.Domain.Entities;

public class ProductAttributeValue : BaseEntity
{
    public string Value { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int CategoryAttributeId { get; set; }
    public Product Product { get; set; } = null!;
    public CategoryAttribute CategoryAttribute { get; set; } = null!;
}
