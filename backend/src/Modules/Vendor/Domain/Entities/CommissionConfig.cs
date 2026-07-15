namespace CommerceHub.Modules.Vendor.Domain.Entities;

public class CommissionConfig : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Global";
    public int? TargetId { get; set; }
    public decimal Rate { get; set; }
}
