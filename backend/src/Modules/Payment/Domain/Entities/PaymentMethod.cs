namespace CommerceHub.Modules.Payment.Domain.Entities;

public class PaymentMethod : BaseEntity
{
    public int UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? MaskedCardNumber { get; set; }
    public string? ExpiryDate { get; set; }
    public string? CardHolderName { get; set; }
    public string? BillingAddress { get; set; }
    public bool IsDefault { get; set; }
}
