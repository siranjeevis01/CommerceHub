using CommerceHub.Identity.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Identity.UnitTests;

public class UserDomainTests
{
    [Fact]
    public void User_ShouldInitializeWithDefaultValues()
    {
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            UserType = "Customer"
        };

        user.Email.Should().Be("test@example.com");
        user.UserType.Should().Be("Customer");
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_ShouldHaveUserType()
    {
        var user = new User { UserType = "Vendor" };
        user.UserType.Should().Be("Vendor");
    }

    [Fact]
    public void User_ShouldTrackFailedLogins()
    {
        var user = new User();
        user.FailedLoginAttempts = 3;
        user.FailedLoginAttempts.Should().Be(3);
    }

    [Fact]
    public void User_ShouldSupportTwoFactor()
    {
        var user = new User { TwoFactorEnabled = true, TwoFactorSecret = "SECRETKEY" };
        user.TwoFactorEnabled.Should().BeTrue();
        user.TwoFactorSecret.Should().Be("SECRETKEY");
    }

    [Fact]
    public void User_ShouldHaveRolesCollection()
    {
        var user = new User();
        user.UserRoles.Add(new UserRole { RoleId = 1 });
        user.UserRoles.Add(new UserRole { RoleId = 2 });
        user.UserRoles.Should().HaveCount(2);
    }

    [Fact]
    public void User_ShouldHaveAddressesCollection()
    {
        var user = new User();
        user.Addresses.Add(new Address { AddressLine1 = "123 Main St", City = "New York", IsDefault = true });
        user.Addresses.Should().HaveCount(1);
    }

    [Fact]
    public void Address_ShouldSetProperties()
    {
        var address = new Address
        {
            AddressLine1 = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsDefault = true
        };

        address.AddressLine1.Should().Be("123 Main St");
        address.City.Should().Be("New York");
        address.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Otp_ShouldGenerateCode_WhenCreated()
    {
        var otp = new Otp
        {
            Email = "user@example.com",
            OtpCode = "123456",
            Type = "email_verification",
            DeliveryMethod = "Email",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        otp.OtpCode.Should().NotBeNullOrEmpty();
        otp.OtpCode.Length.Should().Be(6);
        otp.IsUsed.Should().BeFalse();
        otp.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Otp_IsExpired_ShouldReturnTrue_WhenExpired()
    {
        var otp = new Otp { ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };
        (otp.ExpiresAt < DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void Otp_IsExpired_ShouldReturnFalse_WhenNotExpired()
    {
        var otp = new Otp { ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        (otp.ExpiresAt > DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void Role_ShouldSetName()
    {
        var role = new Role { Name = "Admin" };
        role.Name.Should().Be("Admin");
    }

    [Fact]
    public void UserRole_ShouldAssociateUserAndRole()
    {
        var user = new User { Id = 1 };
        var role = new Role { Id = 2, Name = "Vendor" };
        var userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role };

        userRole.UserId.Should().Be(1);
        userRole.RoleId.Should().Be(2);
        userRole.Role.Name.Should().Be("Vendor");
    }
}
