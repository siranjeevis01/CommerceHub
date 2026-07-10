namespace CommerceHub.Payment.Domain.Entities;

public class GiftCard : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal RemainingBalance { get; set; }
    public int? UserId { get; set; }
    public DateTime ExpiryDate { get; set; }
}
