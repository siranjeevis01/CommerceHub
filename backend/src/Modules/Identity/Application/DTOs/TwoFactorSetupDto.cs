namespace CommerceHub.Modules.Identity.Application.DTOs;

public record TwoFactorSetupDto
{
    public string Secret { get; init; } = string.Empty;
    public string QrCodeUri { get; init; } = string.Empty;
}
