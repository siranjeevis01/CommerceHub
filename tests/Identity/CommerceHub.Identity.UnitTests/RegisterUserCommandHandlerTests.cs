using AutoMapper;
using CommerceHub.Identity.Application.Commands.Auth;
using CommerceHub.Identity.Application.Common.Interfaces;
using CommerceHub.Identity.Application.Common.Models;
using CommerceHub.Identity.Application.DTOs;
using CommerceHub.Identity.Domain.Entities;
using CommerceHub.TestBase;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CommerceHub.Identity.UnitTests;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IIdentityDbContext> _contextMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _contextMock = new Mock<IIdentityDbContext>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _mapperMock = new Mock<IMapper>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new RegisterUserCommandHandler(
            _contextMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _mapperMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenEmailDoesNotExist()
    {
        var users = new List<User>().AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new RegisterUserCommand
        {
            Email = "new@test.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var tokenResult = new JwtTokenResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _passwordHasherMock.Setup(x => x.Hash(command.Password)).Returns("hashed_password");
        _jwtServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenResult);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var authResponse = new AuthResponse
        {
            UserId = 1,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            AccessToken = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresAt = tokenResult.ExpiresAt
        };
        _mapperMock.Setup(x => x.Map<AuthResponse>(It.IsAny<User>())).Returns(authResponse);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        _contextMock.Verify(x => x.Users.Add(It.IsAny<User>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyExists()
    {
        var existingUser = new User { Email = "existing@test.com" };
        var users = new List<User> { existingUser }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new RegisterUserCommand { Email = "existing@test.com" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A user with this email already exists.");
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_WhenRegistering()
    {
        var users = new List<User>().AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new RegisterUserCommand
        {
            Email = "hash@test.com",
            Password = "SecurePass1!",
            FirstName = "Hash",
            LastName = "Test"
        };

        _passwordHasherMock.Setup(x => x.Hash("SecurePass1!")).Returns("hashed_value");
        _jwtServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JwtTokenResult { AccessToken = "tok", RefreshToken = "rt", ExpiresAt = DateTime.UtcNow });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        User? capturedUser = null;
        _contextMock.Setup(x => x.Users.Add(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u);

        _mapperMock.Setup(x => x.Map<AuthResponse>(It.IsAny<User>()))
            .Returns((User u) => new AuthResponse { Email = u.Email, FirstName = u.FirstName });

        await _handler.Handle(command, CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be("hashed_value");
        _passwordHasherMock.Verify(x => x.Hash("SecurePass1!"), Times.Once);
    }
}
