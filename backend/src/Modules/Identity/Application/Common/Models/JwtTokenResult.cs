namespace CommerceHub.Modules.Identity.Application.Common.Models;

public record JwtTokenResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
