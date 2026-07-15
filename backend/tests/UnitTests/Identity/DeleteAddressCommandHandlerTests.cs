using CommerceHub.Modules.Identity.Application.Commands.Address;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Domain.Entities;
using CommerceHub.TestBase;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CommerceHub.Modules.Identity.UnitTests;

public class DeleteAddressCommandHandlerTests
{
    private readonly Mock<IIdentityDbContext> _contextMock;
    private readonly DeleteAddressCommandHandler _handler;

    public DeleteAddressCommandHandlerTests()
    {
        _contextMock = new Mock<IIdentityDbContext>();
        _handler = new DeleteAddressCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteAddress_WhenAddressExists()
    {
        var address = new Address
        {
            Id = 1,
            AddressLine1 = "123 Main St",
            IsDeleted = false,
            IsActive = true
        };
        var addresses = new List<Address> { address }.AsQueryable();
        var mockAddresses = addresses.BuildMockDbSet();
        _contextMock.Setup(x => x.Addresses).Returns(mockAddresses.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteAddressCommand { Id = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        address.IsDeleted.Should().BeTrue();
        address.IsActive.Should().BeFalse();
        address.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAddressNotFound()
    {
        var addresses = new List<Address>().AsQueryable();
        var mockAddresses = addresses.BuildMockDbSet();
        _contextMock.Setup(x => x.Addresses).Returns(mockAddresses.Object);

        var command = new DeleteAddressCommand { Id = 999 };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Address not found.");
    }
}
