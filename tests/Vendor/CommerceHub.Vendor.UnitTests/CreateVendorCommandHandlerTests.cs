using AutoMapper;
using CommerceHub.Vendor.Application.Commands;
using CommerceHub.Vendor.Application.Common.Interfaces;
using CommerceHub.Vendor.Domain.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.TestBase;

namespace CommerceHub.Vendor.UnitTests;

public class CreateVendorCommandHandlerTests
{
    private readonly Mock<IVendorDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly CreateVendorCommandHandler _handler;

    public CreateVendorCommandHandlerTests()
    {
        _contextMock = new Mock<IVendorDbContext>();
        _mapperMock = new Mock<IMapper>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new CreateVendorCommandHandler(_contextMock.Object, _mapperMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateVendor_WhenCommandIsValid()
    {
        var vendors = new List<VendorProfile>().AsQueryable();
        var mockVendors = vendors.BuildMockDbSet();
        _contextMock.Setup(x => x.Vendors).Returns(mockVendors.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateVendorCommand
        {
            StoreName = "Test Store",
            StoreDescription = "A test vendor store",
            BusinessEmail = "vendor@test.com",
            BusinessPhone = "+1234567890",
            BusinessAddress = "123 Vendor St",
            GSTNumber = "GST123",
            PANNumber = "PAN456",
            BusinessType = "Individual",
            UserId = 1,
            CommissionRate = 0.05m
        };

        VendorProfile? capturedVendor = null;
        _contextMock.Setup(x => x.Vendors.Add(It.IsAny<VendorProfile>()))
            .Callback<VendorProfile>(v => { v.Id = 1; capturedVendor = v; });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeGreaterThan(0);
        capturedVendor.Should().NotBeNull();
        capturedVendor!.StoreName.Should().Be("Test Store");
        capturedVendor.VerificationStatus.Should().Be("Pending");
        capturedVendor.IsActive.Should().BeTrue();
        capturedVendor.CommissionRate.Should().Be(0.05m);
        capturedVendor.UserId.Should().Be(1);
        _contextMock.Verify(x => x.Vendors.Add(It.IsAny<VendorProfile>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<VendorCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateVendorWithPendingVerification()
    {
        var vendors = new List<VendorProfile>().AsQueryable();
        var mockVendors = vendors.BuildMockDbSet();
        _contextMock.Setup(x => x.Vendors).Returns(mockVendors.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        VendorProfile? capturedVendor = null;
        _contextMock.Setup(x => x.Vendors.Add(It.IsAny<VendorProfile>()))
            .Callback<VendorProfile>(v => capturedVendor = v);

        var command = new CreateVendorCommand
        {
            StoreName = "New Store",
            UserId = 1
        };

        await _handler.Handle(command, CancellationToken.None);

        capturedVendor!.VerificationStatus.Should().Be("Pending");
    }
}
