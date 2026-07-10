using System;

namespace CommerceHub.Shared.Contracts.Events;

public record UserRegistered
{
    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}
