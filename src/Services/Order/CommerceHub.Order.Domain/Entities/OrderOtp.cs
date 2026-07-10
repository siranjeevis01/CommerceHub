namespace CommerceHub.Order.Domain.Entities;

public class OrderOtp : BaseEntity
{
    public int OrderId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}
