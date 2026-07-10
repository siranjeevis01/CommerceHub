using CommerceHub.Identity.Domain.Entities;
using CommerceHub.Identity.Infrastructure.Data;
using CommerceHub.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommerceHub.Identity.IntegrationTests;

public class IdentityDbContextTests : IntegrationTestBase
{
    [Fact]
    public async Task CanCreateAndRetrieveUser()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            UserType = "Customer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var retrieved = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be("test@example.com");
        retrieved.UserType.Should().Be("Customer");
    }

    [Fact]
    public async Task CanCreateUserWithRole()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var role = new Role { Name = "Customer" };
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var user = new User
        {
            Email = "roleuser@example.com",
            Username = "roleuser",
            FirstName = "Role",
            LastName = "User",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            UserType = "Customer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UserRoles = new List<UserRole> { new() { RoleId = role.Id } }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var retrieved = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == "roleuser@example.com");

        retrieved.Should().NotBeNull();
        retrieved!.UserRoles.Should().HaveCount(1);
        retrieved.UserRoles.First().Role.Name.Should().Be("Customer");
    }
}
