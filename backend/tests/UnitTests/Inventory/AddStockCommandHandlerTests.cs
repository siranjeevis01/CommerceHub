using AutoMapper;
using CommerceHub.Modules.Inventory.Application.Commands;
using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Application.DTOs;
using CommerceHub.Modules.Inventory.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.TestBase;
using InventoryEntity = CommerceHub.Modules.Inventory.Domain.Entities.Inventory;

namespace CommerceHub.Modules.Inventory.UnitTests;

public class AddStockCommandHandlerTests
{
    private readonly Mock<IInventoryDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AddStockCommandHandler _handler;

    public AddStockCommandHandlerTests()
    {
        _contextMock = new Mock<IInventoryDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new AddStockCommandHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddStock_WhenInventoryExists()
    {
        var warehouse = new Warehouse { Id = 1, Name = "Main Warehouse" };
        var inventory = new InventoryEntity
        {
            Id = 1,
            ProductId = 1,
            VariantId = null,
            WarehouseId = 1,
            StockQuantity = 50,
            ReservedQuantity = 10,
            LowStockThreshold = 20,
            Warehouse = warehouse
        };
        var inventories = new List<InventoryEntity> { inventory }.AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<StockLevelDto>(It.IsAny<InventoryEntity>()))
            .Returns((InventoryEntity i) => new StockLevelDto
            {
                ProductId = i.ProductId,
                StockQuantity = i.StockQuantity,
                ReservedQuantity = i.ReservedQuantity,
                AvailableQuantity = i.AvailableQuantity
            });

        var command = new AddStockCommand(1, null, 1, 25);

        var result = await _handler.Handle(command, CancellationToken.None);

        inventory.StockQuantity.Should().Be(75);
        result.StockQuantity.Should().Be(75);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewInventory_WhenNotFound()
    {
        var inventories = new List<InventoryEntity>().AsQueryable();
        var mockInventories = inventories.BuildMockDbSet();
        _contextMock.Setup(x => x.Inventories).Returns(mockInventories.Object);

        var stockMovements = new List<InventoryTransaction>().AsQueryable();
        var mockMovements = stockMovements.BuildMockDbSet();
        _contextMock.Setup(x => x.StockMovements).Returns(mockMovements.Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<StockLevelDto>(It.IsAny<InventoryEntity>()))
            .Returns((InventoryEntity i) => new StockLevelDto
            {
                ProductId = i.ProductId,
                StockQuantity = i.StockQuantity,
                ReservedQuantity = i.ReservedQuantity,
                AvailableQuantity = i.AvailableQuantity
            });

        InventoryEntity? added = null;
        _contextMock.Setup(x => x.Inventories.Add(It.IsAny<InventoryEntity>()))
            .Callback<InventoryEntity>(i => added = i);

        var command = new AddStockCommand(999, null, 1, 100);

        var result = await _handler.Handle(command, CancellationToken.None);

        added.Should().NotBeNull();
        added!.ProductId.Should().Be(999);
        added.StockQuantity.Should().Be(100);
        added.ReservedQuantity.Should().Be(0);
        added.LowStockThreshold.Should().Be(10);
    }
}
