using CommerceHub.Infrastructure.Configurations;
using CommerceHub.Modules.Identity.Domain.Entities;
using CommerceHub.Modules.Identity.Infrastructure.Data;
using CommerceHub.Modules.Product.Domain.Entities;
using CommerceHub.Modules.Product.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommerceHub.Api.Seeder;

public class DatabaseSeeder
{
    private readonly IdentityDbContext _identityCtx;
    private readonly ProductDbContext _productCtx;
    private readonly SeedOptions _seedOpts;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IdentityDbContext identityCtx,
        ProductDbContext productCtx,
        IOptions<SeedOptions> seedOpts,
        ILogger<DatabaseSeeder> logger)
    {
        _identityCtx = identityCtx;
        _productCtx = productCtx;
        _seedOpts = seedOpts.Value;
        _logger = logger;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        await SeedIdentityAsync(cancellationToken);
        await SeedProductCatalogAsync(cancellationToken);
    }

    private async Task SeedIdentityAsync(CancellationToken cancellationToken)
    {
        if (await _identityCtx.Users.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Identity data already seeded, skipping");
            return;
        }

        _logger.LogInformation("Seeding identity data...");

        var roles = new[]
        {
            new Role { Name = "SuperAdmin", Description = "Full system access", CreatedAt = DateTime.UtcNow },
            new Role { Name = "Admin", Description = "Admin access", CreatedAt = DateTime.UtcNow },
            new Role { Name = "Vendor", Description = "Vendor access", CreatedAt = DateTime.UtcNow },
            new Role { Name = "Customer", Description = "Customer access", CreatedAt = DateTime.UtcNow },
            new Role { Name = "Support", Description = "Support access", CreatedAt = DateTime.UtcNow }
        };

        _identityCtx.Roles.AddRange(roles);
        await _identityCtx.SaveChangesAsync(cancellationToken);

        var superAdminRole = _identityCtx.Roles.First(r => r.Name == "SuperAdmin");
        var adminRole = _identityCtx.Roles.First(r => r.Name == "Admin");

        var adminUser = new User
        {
            Email = _seedOpts.AdminEmail,
            Username = "admin",
            FirstName = "Super",
            LastName = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(_seedOpts.AdminPassword),
            UserType = "Admin",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _identityCtx.Users.Add(adminUser);
        await _identityCtx.SaveChangesAsync(cancellationToken);

        _identityCtx.UserRoles.Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = superAdminRole.Id
        });

        _identityCtx.UserRoles.Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        });

        await _identityCtx.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Identity seeded: roles and admin user created ({Email})", _seedOpts.AdminEmail);
    }

    private async Task SeedProductCatalogAsync(CancellationToken cancellationToken)
    {
        if (await _productCtx.Categories.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Product catalog already seeded, skipping");
            return;
        }

        _logger.LogInformation("Seeding product catalog...");

        var electronics = new Category
        {
            Name = "Electronics",
            Slug = "electronics",
            Description = "Electronic devices and accessories",
            DisplayOrder = 1,
            CreatedAt = DateTime.UtcNow,
            Attributes =
            [
                new CategoryAttribute { Name = "Brand", Slug = "brand", DataType = "string", IsRequired = true, IsFilterable = true, DisplayOrder = 1 },
                new CategoryAttribute { Name = "Warranty", Slug = "warranty", DataType = "string", IsRequired = false, IsFilterable = false, DisplayOrder = 2 },
                new CategoryAttribute { Name = "Color", Slug = "color", DataType = "string", IsRequired = false, IsFilterable = true, DisplayOrder = 3, Options = "Black,White,Blue,Red,Gray" }
            ]
        };

        var clothing = new Category
        {
            Name = "Clothing",
            Slug = "clothing",
            Description = "Apparel and fashion items",
            DisplayOrder = 2,
            CreatedAt = DateTime.UtcNow,
            Attributes =
            [
                new CategoryAttribute { Name = "Size", Slug = "size", DataType = "string", IsRequired = true, IsFilterable = true, DisplayOrder = 1, Options = "XS,S,M,L,XL,XXL" },
                new CategoryAttribute { Name = "Material", Slug = "material", DataType = "string", IsRequired = false, IsFilterable = true, DisplayOrder = 2 },
                new CategoryAttribute { Name = "Color", Slug = "color", DataType = "string", IsRequired = true, IsFilterable = true, DisplayOrder = 3, Options = "Black,White,Blue,Red,Green" }
            ]
        };

        var homeGarden = new Category
        {
            Name = "Home & Garden",
            Slug = "home-garden",
            Description = "Home improvement and garden supplies",
            DisplayOrder = 3,
            CreatedAt = DateTime.UtcNow,
            Attributes =
            [
                new CategoryAttribute { Name = "Room", Slug = "room", DataType = "string", IsRequired = false, IsFilterable = true, DisplayOrder = 1, Options = "Living Room,Bedroom,Kitchen,Bathroom,Garden" },
                new CategoryAttribute { Name = "Material", Slug = "material", DataType = "string", IsRequired = false, IsFilterable = true, DisplayOrder = 2 }
            ]
        };

        _productCtx.Categories.AddRange(electronics, clothing, homeGarden);
        await _productCtx.SaveChangesAsync(cancellationToken);

        var brand = new Brand
        {
            Name = "CommerceHub Default",
            Description = "Default marketplace brand",
            CreatedAt = DateTime.UtcNow
        };
        _productCtx.Brands.Add(brand);
        await _productCtx.SaveChangesAsync(cancellationToken);

        var sampleProducts = new Product[]
        {
            new()
            {
                Name = "Wireless Bluetooth Headphones",
                Slug = "wireless-bluetooth-headphones",
                SKU = "WBH-001",
                Price = 49.99m,
                ComparePrice = 79.99m,
                ShortDescription = "Premium wireless headphones with noise cancellation",
                StockQuantity = 150,
                StockStatus = "InStock",
                IsFeatured = true,
                IsPublished = true,
                CategoryId = electronics.Id,
                VendorId = 1,
                BrandId = brand.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Classic Cotton T-Shirt",
                Slug = "classic-cotton-t-shirt",
                SKU = "CTS-001",
                Price = 19.99m,
                ComparePrice = 29.99m,
                ShortDescription = "Comfortable 100% cotton t-shirt",
                StockQuantity = 500,
                StockStatus = "InStock",
                IsFeatured = false,
                IsPublished = true,
                CategoryId = clothing.Id,
                VendorId = 1,
                BrandId = brand.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Smart LED Desk Lamp",
                Slug = "smart-led-desk-lamp",
                SKU = "SDL-001",
                Price = 34.99m,
                ShortDescription = "Adjustable LED desk lamp with touch controls",
                StockQuantity = 80,
                StockStatus = "InStock",
                IsFeatured = true,
                IsPublished = true,
                CategoryId = homeGarden.Id,
                VendorId = 1,
                BrandId = brand.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        _productCtx.Products.AddRange(sampleProducts);
        await _productCtx.SaveChangesAsync(cancellationToken);

        var electronicsBrandAttr = _productCtx.CategoryAttributes.First(a => a.CategoryId == electronics.Id && a.Slug == "brand");
        var clothingSizeAttr = _productCtx.CategoryAttributes.First(a => a.CategoryId == clothing.Id && a.Slug == "size");
        var clothingMaterialAttr = _productCtx.CategoryAttributes.First(a => a.CategoryId == clothing.Id && a.Slug == "material");

        var attributeValues = new ProductAttributeValue[]
        {
            new() { ProductId = sampleProducts[0].Id, CategoryAttributeId = electronicsBrandAttr.Id, Value = "CommerceHub", CreatedAt = DateTime.UtcNow },
            new() { ProductId = sampleProducts[1].Id, CategoryAttributeId = clothingSizeAttr.Id, Value = "M", CreatedAt = DateTime.UtcNow },
            new() { ProductId = sampleProducts[1].Id, CategoryAttributeId = clothingMaterialAttr.Id, Value = "100% Cotton", CreatedAt = DateTime.UtcNow }
        };

        _productCtx.ProductAttributeValues.AddRange(attributeValues);
        await _productCtx.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product catalog seeded: 3 categories, 1 brand, 3 sample products");
    }
}
