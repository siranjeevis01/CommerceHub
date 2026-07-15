using CommerceHub.Modules.Inventory.Application.Commands;
using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Domain.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.Shared.Contracts.Events;
using CommerceHub.TestBase;
using InventoryEntity = CommerceHub.Modules.Inventory.Domain.Entities.Inventory;

namespace CommerceHub.Modules.Inventory.UnitTests;

public class ReserveStockCommandHandlerTests
{
    private readonly Mock<IInventoryDbContext> _contextMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ReserveStockCommandHandler _handler;

    public ReserveStockCommandHandlerTests()
    {
        _contextMock = new Mock<IInventoryDbContext>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new ReserveStockCommandHandler(_contextMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReserveStock_WhenStockAvailable()
    {
        var inventory = new InventoryEntity
        {
            Id = 1,
            ProductId = 1,
            VariantId = null,
            WarehouseId = 1,
            StockQuantity = 100,
            ReservedQuantity = 0,
            LowStockThreshold = 10
        };
        var inventories = new List<InventoryEntity> { inventory }.AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new ReserveStockCommand(1, new List<ReserveStockItem>
        {
            new(1, null, 10)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        inventory.ReservedQuantity.Should().Be(10);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.IsAny<InventoryReserved>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInsufficientStock()
    {
        var inventory = new InventoryEntity
        {
            Id = 1,
            ProductId = 1,
            StockQuantity = 5,
            ReservedQuantity = 0,
            LowStockThreshold = 10
        };
        var inventories = new List<InventoryEntity> { inventory }.AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new ReserveStockCommand(1, new List<ReserveStockItem>
        {
            new(1, null, 10)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Insufficient stock for some items");
        _publishEndpointMock.Verify(x => x.Publish(
            It.IsAny<InventoryFailed>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInventoryNotFound()
    {
        var inventories = new List<InventoryEntity>().AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new ReserveStockCommand(1, new List<ReserveStockItem>
        {
            new(999, null, 1)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldHandleMultipleItems_PartialFailure()
    {
        var inv1 = new InventoryEntity { Id = 1, ProductId = 1, StockQuantity = 10, ReservedQuantity = 0 };
        var inv2 = new InventoryEntity { Id = 2, ProductId = 2, StockQuantity = 0, ReservedQuantity = 0 };
        var inventories = new List<InventoryEntity> { inv1, inv2 }.AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new ReserveStockCommand(1, new List<ReserveStockItem>
        {
            new(1, null, 5),
            new(2, null, 5)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        inv1.ReservedQuantity.Should().Be(5);
        inv2.ReservedQuantity.Should().Be(0);
    }
}
