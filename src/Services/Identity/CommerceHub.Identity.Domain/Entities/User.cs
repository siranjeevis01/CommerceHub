namespace CommerceHub.Identity.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string UserType { get; set; } = "Customer";
    public string? TwoFactorSecret { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? PreferredCurrency { get; set; }
    public string? PreferredCulture { get; set; }
    public int? VendorId { get; set; }
    public int? FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool EmailConfirmed { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
