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

public class UpdateAddressCommandHandlerTests
{
    private readonly Mock<IIdentityDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateAddressCommandHandler _handler;

    public UpdateAddressCommandHandlerTests()
    {
        _contextMock = new Mock<IIdentityDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new UpdateAddressCommandHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAddress_WhenAddressExists()
    {
        var address = new Address
        {
            Id = 1,
            UserId = 1,
            AddressLine1 = "Old Line",
            City = "Old City",
            State = "OS",
            PostalCode = "00000",
            Country = "Old",
            IsDefault = false,
            IsDeleted = false
        };
        var addresses = new List<Address> { address }.AsQueryable();
        var mockAddresses = addresses.BuildMockDbSet();
        _contextMock.Setup(x => x.Addresses).Returns(mockAddresses.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateAddressCommand
        {
            Id = 1,
            AddressLine1 = "New Line",
            City = "New City",
            PostalCode = "11111"
        };

        _mapperMock.Setup(x => x.Map<AddressDto>(It.IsAny<Address>()))
            .Returns((Address a) => new AddressDto
            {
                AddressLine1 = a.AddressLine1,
                City = a.City,
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AddressLine1.Should().Be("New Line");
        result.City.Should().Be("New City");
        address.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAddressNotFound()
    {
        var addresses = new List<Address>().AsQueryable();
        var mockAddresses = addresses.BuildMockDbSet();
        _contextMock.Setup(x => x.Addresses).Returns(mockAddresses.Object);

        var command = new UpdateAddressCommand { Id = 999, AddressLine1 = "New" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Address not found.");
    }

    [Fact]
    public async Task Handle_ShouldHandleSettingDefault_WhenUpdating()
    {
        var address = new Address
        {
            Id = 1,
            UserId = 1,
            AddressLine1 = "Not Default",
            City = "City",
            PostalCode = "12345",
            Country = "US",
            IsDefault = false,
            IsDeleted = false
        };
        var otherDefault = new Address
        {
            Id = 2,
            UserId = 1,
            AddressLine1 = "Other Default",
            IsDefault = true,
            IsDeleted = false
        };
        var addresses = new List<Address> { address, otherDefault }.AsQueryable();
        var mockAddresses = addresses.BuildMockDbSet();
        _contextMock.Setup(x => x.Addresses).Returns(mockAddresses.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<AddressDto>(It.IsAny<Address>()))
            .Returns((Address a) => new AddressDto
            {
                AddressLine1 = a.AddressLine1,
                IsDefault = a.IsDefault,
                City = a.City
            });

        var command = new UpdateAddressCommand { Id = 1, IsDefault = true };
        var result = await _handler.Handle(command, CancellationToken.None);

        address.IsDefault.Should().BeTrue();
        otherDefault.IsDefault.Should().BeFalse();
        result.IsDefault.Should().BeTrue();
    }
}
