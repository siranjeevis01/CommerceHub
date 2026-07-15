namespace CommerceHub.Modules.Cms.Domain.Entities;

public class Campaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? FixedDiscount { get; set; }
    public string? Description { get; set; }
}
