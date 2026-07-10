namespace CommerceHub.Identity.Domain.Entities;

public class Address : BaseEntity
{
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? AddressType { get; set; }
    public bool IsDefault { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
