using CommerceHub.Identity.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Identity.UnitTests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher;

    public PasswordHasherTests()
    {
        _hasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_ShouldReturnCombinedSaltAndHash()
    {
        var storedHash = _hasher.Hash("SecurePassword123!");

        storedHash.Should().NotBeNullOrEmpty();
        storedHash.Should().Contain(":");
        var parts = storedHash.Split(':');
        parts.Should().HaveCount(2);
    }

    [Fact]
    public void Hash_ShouldProduceDifferentHashes_ForSamePassword()
    {
        var hash1 = _hasher.Hash("SamePassword");
        var hash2 = _hasher.Hash("SamePassword");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordCorrect()
    {
        var password = "MySecureP@ss!";
        var storedHash = _hasher.Hash(password);

        var result = _hasher.Verify(password, storedHash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordIncorrect()
    {
        var storedHash = _hasher.Hash("CorrectPassword");

        var result = _hasher.Verify("WrongPassword", storedHash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenHashInvalid()
    {
        var result = _hasher.Verify("AnyPassword", "invalid_hash");

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenHashIsEmpty()
    {
        var result = _hasher.Verify("password", string.Empty);

        result.Should().BeFalse();
    }
}
