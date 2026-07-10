using AutoMapper;
using CommerceHub.Product.Application.Commands;
using CommerceHub.Product.Application.Common.Interfaces;
using CommerceHub.Product.Domain.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.TestBase;
using ProductEntity = CommerceHub.Product.Domain.Entities.Product;

namespace CommerceHub.Product.UnitTests;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProductSearchService> _searchServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _contextMock = new Mock<IProductDbContext>();
        _mapperMock = new Mock<IMapper>();
        _searchServiceMock = new Mock<IProductSearchService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new CreateProductCommandHandler(_contextMock.Object, _mapperMock.Object, _searchServiceMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_AndReturnId()
    {
        var products = new List<ProductEntity>().AsQueryable();
        var mockProducts = products.BuildMockDbSet();
        _contextMock.Setup(x => x.Products).Returns(mockProducts.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Price = 99.99m,
            CategoryId = 1,
            VendorId = 1,
            Description = "A test product",
            SKU = "TST-001",
            Variants = new List<CreateProductVariantDto>()
        };

        var productEntity = new ProductEntity
        {
            Id = 0,
            Name = command.Name,
            Price = command.Price,
            CategoryId = command.CategoryId
        };

        _mapperMock.Setup(x => x.Map<ProductEntity>(command)).Returns(productEntity);

        ProductEntity? captured = null;
        _contextMock.Setup(x => x.Products.Add(It.IsAny<ProductEntity>()))
            .Callback<ProductEntity>(p =>
            {
                p.Id = 1;
                captured = p;
            });

        _searchServiceMock.Setup(x => x.IndexProductAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(1);
        captured.Should().NotBeNull();
        captured!.Slug.Should().NotBeNullOrEmpty();
        captured.Slug.Should().Contain("test-product");
        _searchServiceMock.Verify(x => x.IndexProductAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddVariants_WhenProvided()
    {
        var products = new List<ProductEntity>().AsQueryable();
        var mockProducts = products.BuildMockDbSet();
        _contextMock.Setup(x => x.Products).Returns(mockProducts.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateProductCommand
        {
            Name = "Product With Variants",
            Price = 50.00m,
            CategoryId = 1,
            VendorId = 1,
            SKU = "VAR-001",
            Variants = new List<CreateProductVariantDto>
            {
                new() { Name = "Red", SKU = "VAR-001-RED", Price = 55.00m, Option1 = "Color", Value1 = "Red" },
                new() { Name = "Blue", SKU = "VAR-001-BLU", Price = 55.00m, Option1 = "Color", Value1 = "Blue" }
            }
        };

        var productEntity = new ProductEntity { Name = command.Name };

        _mapperMock.Setup(x => x.Map<ProductEntity>(command)).Returns(productEntity);
        _mapperMock.Setup(x => x.Map<ProductVariant>(It.IsAny<CreateProductVariantDto>()))
            .Returns((CreateProductVariantDto dto) => new ProductVariant { Name = dto.Name, SKU = dto.SKU, Price = dto.Price });

        _searchServiceMock.Setup(x => x.IndexProductAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        ProductEntity? captured = null;
        _contextMock.Setup(x => x.Products.Add(It.IsAny<ProductEntity>()))
            .Callback<ProductEntity>(p => captured = p);

        var result = await _handler.Handle(command, CancellationToken.None);

        captured!.Variants.Should().HaveCount(2);
    }
}
