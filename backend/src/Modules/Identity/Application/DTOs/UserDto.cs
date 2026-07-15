namespace CommerceHub.Modules.Identity.Application.DTOs;

public record UserDto
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public IList<string> Roles { get; init; } = new List<string>();
    public IList<AddressDto> Addresses { get; init; } = new List<AddressDto>();
}
