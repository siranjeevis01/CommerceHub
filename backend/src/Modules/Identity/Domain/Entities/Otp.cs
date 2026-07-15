namespace CommerceHub.Modules.Identity.Domain.Entities;

public class Otp : BaseEntity
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string OtpCode { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? DeliveryMethod { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int AttemptCount { get; set; }
}
