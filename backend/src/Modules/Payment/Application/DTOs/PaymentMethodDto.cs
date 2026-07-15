namespace CommerceHub.Modules.Payment.Application.DTOs;

public record PaymentMethodDto
{
    public int Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? LastFourDigits { get; init; }
    public string? ExpiryMonth { get; init; }
    public string? ExpiryYear { get; init; }
    public string? CardholderName { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
}
