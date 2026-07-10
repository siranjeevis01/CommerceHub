namespace CommerceHub.Cms.Domain.Entities;

public class RoleMenu : BaseEntity
{
    public int RoleId { get; set; }
    public int MenuId { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public Menu Menu { get; set; } = null!;
}
