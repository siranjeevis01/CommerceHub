using CommerceHub.Product.Domain.Entities;
using FluentAssertions;
using Xunit;
using ProductEntity = CommerceHub.Product.Domain.Entities.Product;

namespace CommerceHub.Product.UnitTests;

public class ProductDomainTests
{
    [Fact]
    public void Product_ShouldSetProperties_WhenCreated()
    {
        var product = new ProductEntity
        {
            Id = 1,
            Name = "Test Product",
            ShortDescription = "A test product",
            Price = 99.99m,
            StockQuantity = 100,
            SKU = "TST-001",
            CategoryId = 1,
            BrandId = 1,
            IsActive = true
        };

        product.Name.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
        product.SKU.Should().Be("TST-001");
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Category_ShouldHaveHierarchy()
    {
        var parentCategory = new Category
        {
            Id = 1,
            Name = "Electronics",
            Slug = "electronics"
        };

        var childCategory = new Category
        {
            Id = 2,
            Name = "Laptops",
            Slug = "laptops",
            ParentCategoryId = 1
        };

        parentCategory.Name.Should().Be("Electronics");
        childCategory.ParentCategoryId.Should().Be(1);
    }

    [Fact]
    public void Review_ShouldCalculateRating()
    {
        var review = new Review
        {
            ProductId = 1,
            UserId = 1,
            Rating = 5,
            Comment = "Excellent product!",
            IsActive = true
        };

        review.Rating.Should().Be(5);
        review.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ProductVariant_ShouldTrackAttributes()
    {
        var variant = new ProductVariant
        {
            ProductId = 1,
            Name = "Red-M",
            SKU = "TST-001-RED",
            Price = 109.99m,
            StockQuantity = 50,
            Attributes = "{\"Color\":\"Red\",\"Size\":\"M\"}"
        };

        variant.SKU.Should().Be("TST-001-RED");
        variant.Name.Should().Be("Red-M");
        variant.Price.Should().Be(109.99m);
    }

    [Fact]
    public void WishlistItem_ShouldSetProperties()
    {
        var item = new WishlistItem
        {
            UserId = 1,
            ProductId = 1
        };

        item.UserId.Should().Be(1);
        item.ProductId.Should().Be(1);
    }
}
