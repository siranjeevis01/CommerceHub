namespace CommerceHub.Modules.Identity.Application.DTOs;

public record AddressDto
{
    public int Id { get; init; }
    public string AddressLine1 { get; init; } = string.Empty;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string? State { get; init; }
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? AddressType { get; init; }
    public bool IsDefault { get; init; }
}
