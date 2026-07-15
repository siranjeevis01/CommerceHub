using CommerceHub.Modules.Product.Domain.Entities;
using CommerceHub.Modules.Product.Infrastructure.Data;
using CommerceHub.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommerceHub.Modules.Product.IntegrationTests;

public class ProductDbContextTests : IntegrationTestBase
{
    [Fact]
    public async Task ProductDbContext_ShouldCreateAndRetrieveProduct()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new ProductDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var brand = new Brand { Name = "TestBrand", LogoUrl = "https://example.com/logo.png", IsActive = true };
        context.Brands.Add(brand);
        await context.SaveChangesAsync();

        var category = new Category { Name = "TestCategory", Slug = "test-category", IsActive = true };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = new global::CommerceHub.Modules.Product.Domain.Entities.Product
        {
            Name = "Integration Test Product",
            Slug = "integration-test-product",
            ShortDescription = "A product for integration testing",
            Price = 49.99m,
            SKU = "INT-TST-001",
            StockQuantity = 100,
            BrandId = brand.Id,
            CategoryId = category.Id,
            IsActive = true,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var retrieved = await context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.SKU == "INT-TST-001");

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Integration Test Product");
        retrieved.Price.Should().Be(49.99m);
        retrieved.Brand!.Name.Should().Be("TestBrand");
        retrieved.Category.Name.Should().Be("TestCategory");
    }

    [Fact]
    public async Task ProductDbContext_ShouldHandleProductVariants()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new ProductDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var product = new global::CommerceHub.Modules.Product.Domain.Entities.Product
        {
            Name = "Variant Product",
            Slug = "variant-product",
            Price = 99.99m,
            SKU = "VAR-001",
            StockQuantity = 50,
            IsActive = true,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Red - Medium",
            SKU = "VAR-001-RM",
            Price = 109.99m,
            StockQuantity = 25,
            Attributes = "{\"Color\":\"Red\",\"Size\":\"M\"}"
        };

        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();

        var retrieved = await context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Variants.Should().HaveCount(1);
        retrieved.Variants.First().SKU.Should().Be("VAR-001-RM");
    }
}
