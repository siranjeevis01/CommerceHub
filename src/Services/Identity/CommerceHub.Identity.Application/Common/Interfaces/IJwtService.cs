using CommerceHub.Identity.Domain.Entities;
using CommerceHub.Identity.Application.Common.Models;

namespace CommerceHub.Identity.Application.Common.Interfaces;

public interface IJwtService
{
    Task<JwtTokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
    Task<string?> ValidateTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
