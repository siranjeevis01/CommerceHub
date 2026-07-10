using AutoMapper;
using CommerceHub.Identity.Application.Commands.Auth;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.Common.Models;
using CommerceHub.Identity.Application.DTOs;
using CommerceHub.Identity.Domain.Entities;
using CommerceHub.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CommerceHub.Identity.UnitTests;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IIdentityDbContext> _contextMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _contextMock = new Mock<IIdentityDbContext>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new LoginUserCommandHandler(
            _contextMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _mapperMock.Object);
    }



    [Fact]
    public async Task Handle_ShouldReturnAuthResponse_WhenCredentialsValid()
    {
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = "hashed_password",
            IsActive = true,
            IsDeleted = false,
            UserRoles = new List<UserRole>()
        };
        var users = new List<User> { user }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();

        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);
        _passwordHasherMock.Setup(x => x.Verify("CorrectPass1!", "hashed_password")).Returns(true);

        var tokenResult = new JwtTokenResult
        {
            AccessToken = "jwt_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        _jwtServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(tokenResult);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var authResponse = new AuthResponse
        {
            UserId = 1,
            Email = "test@test.com",
            AccessToken = "jwt_token",
            RefreshToken = "refresh_token",
            ExpiresAt = tokenResult.ExpiresAt
        };
        _mapperMock.Setup(x => x.Map<AuthResponse>(user)).Returns(authResponse);

        var command = new LoginUserCommand { Email = "test@test.com", Password = "CorrectPass1!" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        var users = new List<User>().AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new LoginUserCommand { Email = "unknown@test.com", Password = "pass" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAccountIsInactive()
    {
        var user = new User
        {
            Email = "inactive@test.com",
            PasswordHash = "hash",
            IsActive = false,
            IsDeleted = false
        };
        var users = new List<User> { user }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new LoginUserCommand { Email = "inactive@test.com", Password = "pass" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account is deactivated.");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAccountIsDeleted()
    {
        var user = new User
        {
            Email = "deleted@test.com",
            PasswordHash = "hash",
            IsActive = true,
            IsDeleted = true
        };
        var users = new List<User> { user }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new LoginUserCommand { Email = "deleted@test.com", Password = "pass" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account is deactivated.");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAccountIsLocked()
    {
        var user = new User
        {
            Email = "locked@test.com",
            PasswordHash = "hash",
            IsActive = true,
            IsDeleted = false,
            LockoutEnd = DateTime.UtcNow.AddMinutes(15)
        };
        var users = new List<User> { user }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new LoginUserCommand { Email = "locked@test.com", Password = "pass" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account is temporarily locked. Try again later.");
    }

    [Fact]
    public async Task Handle_ShouldTrackFailedLogin_WhenPasswordInvalid()
    {
        var user = new User
        {
            Id = 1,
            Email = "fail@test.com",
            PasswordHash = "correct_hash",
            IsActive = true,
            IsDeleted = false,
            FailedLoginAttempts = 0
        };
        var users = new List<User> { user }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        _passwordHasherMock.Setup(x => x.Verify("wrong", "correct_hash")).Returns(false);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new LoginUserCommand { Email = "fail@test.com", Password = "wrong" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid email or password.");

        user.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldLockAccount_AfterFiveFailedAttempts()
    {
        var user = new User
        {
            Id = 1,
            Email = "lockout@test.com",
            PasswordHash = "hash",
            IsActive = true,
            IsDeleted = false,
            FailedLoginAttempts = 4
        };
        var users = new List<User> { user }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        _passwordHasherMock.Setup(x => x.Verify("wrong", "hash")).Returns(false);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new LoginUserCommand { Email = "lockout@test.com", Password = "wrong" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();

        user.LockoutEnd.Should().NotBeNull();
        user.FailedLoginAttempts.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldResetFailedAttempts_OnSuccessfulLogin()
    {
        var user = new User
        {
            Id = 1,
            Email = "reset@test.com",
            PasswordHash = "hash",
            IsActive = true,
            IsDeleted = false,
            FailedLoginAttempts = 2,
            LockoutEnd = null
        };
        var users = new List<User> { user }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        _passwordHasherMock.Setup(x => x.Verify("correct", "hash")).Returns(true);
        _jwtServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JwtTokenResult { AccessToken = "tok", RefreshToken = "rt", ExpiresAt = DateTime.UtcNow });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<AuthResponse>(user)).Returns(new AuthResponse());

        var command = new LoginUserCommand { Email = "reset@test.com", Password = "correct" };
        await _handler.Handle(command, CancellationToken.None);

        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }
}
