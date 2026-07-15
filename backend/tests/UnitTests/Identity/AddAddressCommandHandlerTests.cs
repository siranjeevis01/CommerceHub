using AutoMapper;
using CommerceHub.Modules.Identity.Application.Commands.Address;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Application.DTOs;
using CommerceHub.Modules.Identity.Domain.Entities;
using CommerceHub.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CommerceHub.Modules.Identity.UnitTests;

public class AddAddressCommandHandlerTests
{
    private readonly Mock<IIdentityDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AddAddressCommandHandler _handler;

    public AddAddressCommandHandlerTests()
    {
        _contextMock = new Mock<IIdentityDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new AddAddressCommandHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddAddress_WhenUserExists()
    {
        var users = new List<User> { new User { Id = 1 } }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var addresses = new List<Address>().AsQueryable();
        var mockAddresses = addresses.BuildMockDbSet();
        _contextMock.Setup(x => x.Addresses).Returns(mockAddresses.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new AddAddressCommand
        {
            UserId = 1,
            AddressLine1 = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsDefault = true
        };

        _mapperMock.Setup(x => x.Map<AddressDto>(It.IsAny<Address>()))
            .Returns((Address a) => new AddressDto
            {
                AddressLine1 = a.AddressLine1,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                IsDefault = a.IsDefault
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AddressLine1.Should().Be("123 Main St");
        result.IsDefault.Should().BeTrue();
        _contextMock.Verify(x => x.Addresses.Add(It.IsAny<Address>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotExist()
    {
        var users = new List<User>().AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new AddAddressCommand { UserId = 999, AddressLine1 = "123 St", City = "City" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_ShouldUnsetExistingDefaults_WhenNewAddressIsDefault()
    {
        var user = new User { Id = 1 };
        var users = new List<User> { user }.AsQueryable();
        var mockUsers = users.BuildMockDbSet();
        _contextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var existingDefault = new Address
        {
            Id = 1,
            UserId = 1,
            AddressLine1 = "Old Default",
            IsDefault = true,
            IsDeleted = false
        };
        var addresses = new List<Address> { existingDefault }.AsQueryable();
        var mockAddresses = addresses.BuildMockDbSet();
        _contextMock.Setup(x => x.Addresses).Returns(mockAddresses.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<AddressDto>(It.IsAny<Address>()))
            .Returns((Address a) => new AddressDto { AddressLine1 = a.AddressLine1, IsDefault = a.IsDefault });

        var command = new AddAddressCommand
        {
            UserId = 1,
            AddressLine1 = "New Default",
            City = "City",
            State = "ST",
            PostalCode = "12345",
            Country = "US",
            IsDefault = true
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        existingDefault.IsDefault.Should().BeFalse();
        result.IsDefault.Should().BeTrue();
    }
}
