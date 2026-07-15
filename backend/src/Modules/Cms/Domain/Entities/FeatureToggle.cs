namespace CommerceHub.Modules.Cms.Domain.Entities;

public class FeatureToggle : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string? Description { get; set; }
}
