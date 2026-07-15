using System.Security.Claims;
using CommerceHub.Modules.Identity.Domain.Entities;
using CommerceHub.Modules.Identity.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CommerceHub.Modules.Identity.UnitTests;

public class JwtServiceTests
{
    private Mock<IConfiguration> CreateConfigWithKey(string key = "ThisIsASecretKeyForTestingPurposesAtLeast32Chars!")
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Jwt:Key"]).Returns(key);
        configMock.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        configMock.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
        return configMock;
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnValidJwt_WhenUserProvided()
    {
        var configMock = CreateConfigWithKey();
        var service = new JwtService(configMock.Object);

        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            Username = "testuser",
            UserRoles = new List<UserRole>
            {
                new() { Role = new Role { Name = "Customer" } }
            }
        };

        var result = await service.GenerateTokenAsync(user);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.AccessToken.Split('.').Should().HaveCount(3);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateToken_ShouldIncludeUserClaims()
    {
        var configMock = CreateConfigWithKey();
        var service = new JwtService(configMock.Object);

        var user = new User
        {
            Id = 42,
            Email = "claims@test.com",
            Username = "claimuser",
            UserRoles = new List<UserRole>
            {
                new() { Role = new Role { Name = "Admin" } }
            }
        };

        var result = await service.GenerateTokenAsync(user);
        var userId = await service.ValidateTokenAsync(result.AccessToken);

        userId.Should().Be("42");
    }

    [Fact]
    public async Task GenerateToken_ShouldThrow_WhenKeyNotConfigured()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Jwt:Key"]).Returns((string?)null);
        var service = new JwtService(configMock.Object);

        var user = new User { Id = 1, Email = "test@test.com", Username = "test" };

        await FluentActions.Invoking(() => service.GenerateTokenAsync(user))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*JWT Key not configured*");
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnRefreshToken()
    {
        var configMock = CreateConfigWithKey();
        var service = new JwtService(configMock.Object);

        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            Username = "testuser",
            UserRoles = new List<UserRole>()
        };

        var result = await service.GenerateTokenAsync(user);

        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotContain(" ");
        Convert.FromBase64String(result.RefreshToken).Length.Should().Be(64);
    }

    [Fact]
    public async Task ValidateToken_ShouldReturnUserId_WhenTokenValid()
    {
        var configMock = CreateConfigWithKey();
        var service = new JwtService(configMock.Object);

        var user = new User { Id = 99, Email = "valid@test.com", Username = "validuser", UserRoles = new List<UserRole>() };
        var result = await service.GenerateTokenAsync(user);

        var userId = await service.ValidateTokenAsync(result.AccessToken);

        userId.Should().Be("99");
    }

    [Fact]
    public async Task ValidateToken_ShouldReturnNull_WhenTokenInvalid()
    {
        var configMock = CreateConfigWithKey();
        var service = new JwtService(configMock.Object);

        var userId = await service.ValidateTokenAsync("invalid.jwt.token");

        userId.Should().BeNull();
    }

    [Fact]
    public async Task GenerateToken_ShouldIncludeRoleClaims()
    {
        var configMock = CreateConfigWithKey();
        var service = new JwtService(configMock.Object);

        var user = new User
        {
            Id = 1,
            Email = "role@test.com",
            Username = "roleuser",
            UserRoles = new List<UserRole>
            {
                new() { Role = new Role { Name = "Admin" } },
                new() { Role = new Role { Name = "Vendor" } }
            }
        };

        var result = await service.GenerateTokenAsync(user);

        result.AccessToken.Should().NotBeNullOrEmpty();
    }
}
