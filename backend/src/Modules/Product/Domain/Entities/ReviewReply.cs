using CommerceHub.Shared.Kernel.Entities;

namespace CommerceHub.Modules.Product.Domain.Entities;

public class ReviewReply : BaseEntity
{
    public int ReviewId { get; set; }
    public int VendorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime RepliedAt { get; set; } = DateTime.UtcNow;

    public Review? Review { get; set; }
}
