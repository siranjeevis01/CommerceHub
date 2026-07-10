namespace CommerceHub.Product.Domain.Entities;

public class CategoryAttribute : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
