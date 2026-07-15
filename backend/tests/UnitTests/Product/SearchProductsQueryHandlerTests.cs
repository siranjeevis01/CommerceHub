using AutoMapper;
using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Modules.Product.Application.DTOs;
using CommerceHub.Modules.Product.Application.Queries;
using CommerceHub.Modules.Product.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CommerceHub.TestBase;
using ProductEntity = CommerceHub.Modules.Product.Domain.Entities.Product;

namespace CommerceHub.Modules.Product.UnitTests;

public class SearchProductsQueryHandlerTests
{
    private readonly Mock<IProductDbContext> _contextMock;
    private readonly Mock<IProductSearchService> _searchServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly SearchProductsQueryHandler _handler;

    public SearchProductsQueryHandlerTests()
    {
        _contextMock = new Mock<IProductDbContext>();
        _searchServiceMock = new Mock<IProductSearchService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new SearchProductsQueryHandler(_contextMock.Object, _searchServiceMock.Object, _mapperMock.Object);
    }

    private List<ProductEntity> GetSampleProducts()
    {
        return new List<ProductEntity>
        {
            new()
            {
                Id = 1, Name = "Laptop", Price = 999.99m, CategoryId = 1, VendorId = 1,
                Category = new Category { Id = 1, Name = "Electronics" },
                Reviews = new List<Review>{ new() { Rating = 5 } },
                CreatedAt = DateTime.UtcNow, IsPublished = true
            },
            new()
            {
                Id = 2, Name = "Phone", Price = 699.99m, CategoryId = 1, VendorId = 2,
                Category = new Category { Id = 1, Name = "Electronics" },
                Reviews = new List<Review>{ new() { Rating = 4 } },
                CreatedAt = DateTime.UtcNow.AddDays(-1), IsPublished = true
            },
            new()
            {
                Id = 3, Name = "Book", Price = 19.99m, CategoryId = 2, VendorId = 1,
                Category = new Category { Id = 2, Name = "Books" },
                Reviews = new List<Review>(),
                CreatedAt = DateTime.UtcNow.AddDays(-2), IsPublished = true
            }
        };
    }

    private Mock<DbSet<ProductEntity>> CreateMockProductsDbSet(IQueryable<ProductEntity> data)
    {
        return data.BuildMockDbSet();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllProducts_WhenNoFilter()
    {
        var products = GetSampleProducts().AsQueryable();
        var mockProducts = CreateMockProductsDbSet(products);
        _contextMock.Setup(x => x.Products).Returns(mockProducts.Object);

        var query = new SearchProductsQuery { Page = 1, PageSize = 10 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldFilterByCategory()
    {
        var products = GetSampleProducts().AsQueryable();
        var mockProducts = CreateMockProductsDbSet(products);
        _contextMock.Setup(x => x.Products).Returns(mockProducts.Object);

        var query = new SearchProductsQuery { CategoryId = 2, Page = 1, PageSize = 10 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items.First().CategoryName.Should().Be("Books");
    }

    [Fact]
    public async Task Handle_ShouldFilterByPriceRange()
    {
        var products = GetSampleProducts().AsQueryable();
        var mockProducts = CreateMockProductsDbSet(products);
        _contextMock.Setup(x => x.Products).Returns(mockProducts.Object);

        var query = new SearchProductsQuery { MinPrice = 500, MaxPrice = 1000, Page = 1, PageSize = 10 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldSortByPriceAscending()
    {
        var products = GetSampleProducts().AsQueryable();
        var mockProducts = CreateMockProductsDbSet(products);
        _contextMock.Setup(x => x.Products).Returns(mockProducts.Object);

        var query = new SearchProductsQuery { SortBy = "price_asc", Page = 1, PageSize = 10 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Select(i => i.Price).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_ShouldSortByPriceDescending()
    {
        var products = GetSampleProducts().AsQueryable();
        var mockProducts = CreateMockProductsDbSet(products);
        _contextMock.Setup(x => x.Products).Returns(mockProducts.Object);

        var query = new SearchProductsQuery { SortBy = "price_desc", Page = 1, PageSize = 10 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Select(i => i.Price).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenSearchReturnsNoResults()
    {
        _searchServiceMock.Setup(x => x.SearchAsync("nonexistent", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>().AsReadOnly());

        var query = new SearchProductsQuery { Query = "nonexistent", Page = 1, PageSize = 10 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPaginateResults()
    {
        var products = GetSampleProducts().AsQueryable();
        var mockProducts = CreateMockProductsDbSet(products);
        _contextMock.Setup(x => x.Products).Returns(mockProducts.Object);

        var query = new SearchProductsQuery { Page = 1, PageSize = 2 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }
}
