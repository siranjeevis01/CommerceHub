using CommerceHub.Modules.Inventory.Application.Commands;
using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Domain.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.TestBase;
using InventoryEntity = CommerceHub.Modules.Inventory.Domain.Entities.Inventory;

namespace CommerceHub.Modules.Inventory.UnitTests;

public class DeductStockCommandHandlerTests
{
    private readonly Mock<IInventoryDbContext> _contextMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly DeductStockCommandHandler _handler;

    public DeductStockCommandHandlerTests()
    {
        _contextMock = new Mock<IInventoryDbContext>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new DeductStockCommandHandler(_contextMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeductStock_WhenItemsAvailable()
    {
        var inventory = new InventoryEntity
        {
            Id = 1,
            ProductId = 1,
            StockQuantity = 100,
            ReservedQuantity = 20
        };
        var inventories = new List<InventoryEntity> { inventory }.AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeductStockCommand(1, new List<DeductStockItem>
        {
            new(1, null, 10)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        inventory.StockQuantity.Should().Be(90);
        inventory.ReservedQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldNotExceedAvailableStock()
    {
        var inventory = new InventoryEntity
        {
            Id = 1,
            ProductId = 1,
            StockQuantity = 5,
            ReservedQuantity = 2
        };
        var inventories = new List<InventoryEntity> { inventory }.AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeductStockCommand(1, new List<DeductStockItem>
        {
            new(1, null, 100)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        inventory.StockQuantity.Should().Be(0);
        inventory.ReservedQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldSkip_WhenInventoryNotFound()
    {
        var inventories = new List<InventoryEntity>().AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeductStockCommand(1, new List<DeductStockItem>
        {
            new(999, null, 1)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }
}
