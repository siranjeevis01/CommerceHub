using System.Security.Cryptography;
using System.Text;
using CommerceHub.Cms.Domain.Entities;
using CommerceHub.Cms.Infrastructure.Data;
using CommerceHub.Identity.Domain.Entities;
using CommerceHub.Identity.Infrastructure.Data;
using CommerceHub.Product.Infrastructure.Data;
using CommerceHub.Vendor.Domain.Entities;
using CommerceHub.Vendor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using ProductEntity = CommerceHub.Product.Domain.Entities.Product;
using CategoryEntity = CommerceHub.Product.Domain.Entities.Category;
using BrandEntity = CommerceHub.Product.Domain.Entities.Brand;

namespace CommerceHub.Api.Services;

public class SeedDataService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(IServiceProvider serviceProvider, ILogger<SeedDataService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<SeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var result = new SeedResult();

        using var scope = _serviceProvider.CreateScope();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var productDb = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var vendorDb = scope.ServiceProvider.GetRequiredService<VendorDbContext>();
        var cmsDb = scope.ServiceProvider.GetRequiredService<CmsDbContext>();

        if (await identityDb.Users.AnyAsync(cancellationToken))
        {
            result.Message = "Database already seeded. Admin user exists.";
            result.AlreadySeeded = true;
            return result;
        }

        try
        {
            await SeedIdentityAsync(identityDb, cancellationToken);
            result.IdentitySeeded = true;
            _logger.LogInformation("Identity seed completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Identity seed failed");
            result.Errors.Add($"Identity: {ex.Message}");
        }

        try
        {
            var vendorId = await SeedVendorAsync(vendorDb, cancellationToken);
            result.VendorSeeded = true;
            _logger.LogInformation("Vendor seed completed");

            try
            {
                await SeedProductsAsync(productDb, vendorId, cancellationToken);
                result.ProductsSeeded = true;
                _logger.LogInformation("Products seed completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Products seed failed");
                result.Errors.Add($"Products: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vendor seed failed");
            result.Errors.Add($"Vendor: {ex.Message}");
        }

        try
        {
            await SeedCmsAsync(cmsDb, cancellationToken);
            result.CmsSeeded = true;
            _logger.LogInformation("CMS seed completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CMS seed failed");
            result.Errors.Add($"CMS: {ex.Message}");
        }

        result.Message = result.Errors.Count > 0
            ? $"Seed completed with {result.Errors.Count} error(s)"
            : "Seed completed successfully";

        return result;
    }

    private static string HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return $"{salt}:{hash}";
    }

    private static async Task SeedIdentityAsync(IdentityDbContext db, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var roles = new List<Role>
        {
            new() { Name = "Admin", Description = "Platform administrator", CreatedAt = now, IsActive = true },
            new() { Name = "Vendor", Description = "Seller on the platform", CreatedAt = now, IsActive = true },
            new() { Name = "Customer", Description = "Regular customer", CreatedAt = now, IsActive = true }
        };

        db.Roles.AddRange(roles);
        await db.SaveChangesAsync(ct);

        var adminRole = roles.First(r => r.Name == "Admin");
        var vendorRole = roles.First(r => r.Name == "Vendor");

        var adminUser = new User
        {
            Email = "admin@commercehub.com",
            Username = "admin@commercehub.com",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = HashPassword("Admin@1234"),
            PasswordSalt = string.Empty,
            UserType = "Admin",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = now
        };

        var vendorUser = new User
        {
            Email = "vendor@commercehub.com",
            Username = "vendor@commercehub.com",
            FirstName = "Vendor",
            LastName = "User",
            PasswordHash = HashPassword("Vendor@1234"),
            PasswordSalt = string.Empty,
            UserType = "Vendor",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = now
        };

        db.Users.AddRange(adminUser, vendorUser);
        await db.SaveChangesAsync(ct);

        db.UserRoles.AddRange(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id },
            new UserRole { UserId = vendorUser.Id, RoleId = vendorRole.Id });
        await db.SaveChangesAsync(ct);
    }

    private static async Task<int> SeedVendorAsync(VendorDbContext db, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var vendor = new VendorProfile
        {
            StoreName = "Demo Vendor Store",
            StoreDescription = "Official demo store for CommerceHub platform",
            BusinessEmail = "vendor@commercehub.com",
            BusinessPhone = "+1-555-0100",
            BusinessType = "Electronics & Fashion",
            BusinessAddress = "123 Commerce St, San Francisco, CA 94105",
            VerificationStatus = "Verified",
            CommissionRate = 5.0m,
            UserId = 2,
            CreatedAt = now,
            IsActive = true
        };

        db.Vendors.Add(vendor);
        await db.SaveChangesAsync(ct);
        return vendor.Id;
    }

    private static async Task SeedProductsAsync(ProductDbContext db, int vendorId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var electronics = new CategoryEntity
        {
            Name = "Electronics",
            Slug = "electronics",
            Description = "Electronic devices and gadgets",
            ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400",
            DisplayOrder = 1,
            CreatedAt = now,
            IsActive = true
        };

        var smartphones = new CategoryEntity
        {
            Name = "Smartphones",
            Slug = "smartphones",
            Description = "Mobile phones and accessories",
            ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400",
            DisplayOrder = 1,
            CreatedAt = now,
            IsActive = true
        };

        var laptops = new CategoryEntity
        {
            Name = "Laptops",
            Slug = "laptops",
            Description = "Laptops and notebooks",
            ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400",
            DisplayOrder = 2,
            CreatedAt = now,
            IsActive = true
        };

        var headphones = new CategoryEntity
        {
            Name = "Headphones",
            Slug = "headphones",
            Description = "Headphones and earbuds",
            ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400",
            DisplayOrder = 3,
            CreatedAt = now,
            IsActive = true
        };

        var fashion = new CategoryEntity
        {
            Name = "Fashion",
            Slug = "fashion",
            Description = "Clothing and accessories",
            ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?w=400",
            DisplayOrder = 2,
            CreatedAt = now,
            IsActive = true
        };

        var mensFashion = new CategoryEntity
        {
            Name = "Men",
            Slug = "mens-fashion",
            Description = "Men's clothing and footwear",
            ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=400",
            DisplayOrder = 1,
            CreatedAt = now,
            IsActive = true
        };

        var womensFashion = new CategoryEntity
        {
            Name = "Women",
            Slug = "womens-fashion",
            Description = "Women's clothing and accessories",
            ImageUrl = "https://images.unsplash.com/photo-1487222477894-8943e31ef7b2?w=400",
            DisplayOrder = 2,
            CreatedAt = now,
            IsActive = true
        };

        var homeLiving = new CategoryEntity
        {
            Name = "Home & Living",
            Slug = "home-living",
            Description = "Home decor and living essentials",
            ImageUrl = "https://images.unsplash.com/photo-1556228453-efd6c1ff04f6?w=400",
            DisplayOrder = 3,
            CreatedAt = now,
            IsActive = true
        };

        var sports = new CategoryEntity
        {
            Name = "Sports",
            Slug = "sports",
            Description = "Sports and fitness equipment",
            ImageUrl = "https://images.unsplash.com/photo-1461896836934-bd45ba074e6c?w=400",
            DisplayOrder = 4,
            CreatedAt = now,
            IsActive = true
        };

        var books = new CategoryEntity
        {
            Name = "Books",
            Slug = "books",
            Description = "Books and publications",
            ImageUrl = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400",
            DisplayOrder = 5,
            CreatedAt = now,
            IsActive = true
        };

        db.Categories.AddRange(electronics, smartphones, laptops, headphones,
            fashion, mensFashion, womensFashion, homeLiving, sports, books);
        await db.SaveChangesAsync(ct);

        var samsung = new BrandEntity { Name = "Samsung", LogoUrl = "https://logo.clearbit.com/samsung.com", Description = "Samsung Electronics", CreatedAt = now, IsActive = true };
        var apple = new BrandEntity { Name = "Apple", LogoUrl = "https://logo.clearbit.com/apple.com", Description = "Apple Inc.", CreatedAt = now, IsActive = true };
        var sony = new BrandEntity { Name = "Sony", LogoUrl = "https://logo.clearbit.com/sony.com", Description = "Sony Corporation", CreatedAt = now, IsActive = true };
        var nike = new BrandEntity { Name = "Nike", LogoUrl = "https://logo.clearbit.com/nike.com", Description = "Nike Inc.", CreatedAt = now, IsActive = true };
        var adidas = new BrandEntity { Name = "Adidas", LogoUrl = "https://logo.clearbit.com/adidas.com", Description = "Adidas AG", CreatedAt = now, IsActive = true };

        db.Brands.AddRange(samsung, apple, sony, nike, adidas);
        await db.SaveChangesAsync(ct);

        var products = new List<ProductEntity>
        {
            new()
            {
                Name = "Samsung Galaxy S24 Ultra",
                Slug = "samsung-galaxy-s24-ultra",
                SKU = "SAMSUNG-GS24U",
                Price = 129999,
                ComparePrice = 149999,
                ShortDescription = "Premium flagship smartphone with S Pen",
                LongDescription = "Experience the pinnacle of mobile technology with the Samsung Galaxy S24 Ultra. Featuring a stunning Dynamic AMOLED 2X display, the powerful Snapdragon 8 Gen 3 processor, and an integrated S Pen for ultimate productivity. Capture breathtaking photos with the 200MP camera system and enjoy all-day battery life with Super Fast Charging.",
                StockQuantity = 50,
                MainImageUrl = "https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=600",
                IsPublished = true,
                IsFeatured = true,
                CategoryId = smartphones.Id,
                VendorId = vendorId,
                BrandId = samsung.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "iPhone 15 Pro Max",
                Slug = "iphone-15-pro-max",
                SKU = "APPLE-IP15PM",
                Price = 159999,
                ComparePrice = 179999,
                ShortDescription = "The most powerful iPhone ever",
                LongDescription = "iPhone 15 Pro Max represents the best of Apple's innovation. Built with a titanium design, the A17 Pro chip delivers revolutionary performance. The 48MP camera system with 5x optical zoom captures extraordinary detail. With USB-C, Action Button, and the most durable glass ever in a smartphone, this is the ultimate iPhone experience.",
                StockQuantity = 30,
                MainImageUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=600",
                IsPublished = true,
                IsFeatured = true,
                CategoryId = smartphones.Id,
                VendorId = vendorId,
                BrandId = apple.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Sony WH-1000XM5",
                Slug = "sony-wh-1000xm5",
                SKU = "SONY-WH1000XM5",
                Price = 29999,
                ComparePrice = 34999,
                ShortDescription = "Industry-leading noise cancelling headphones",
                LongDescription = "The Sony WH-1000XM5 redefines noise cancellation with Auto NC Optimizer and eight microphones. Industry-leading noise cancellation automatically adjusts to your environment. Exceptional sound quality with newly designed 30mm drivers and Integrated Processor V1. Enjoy up to 30 hours of wireless listening with quick charge capability.",
                StockQuantity = 100,
                MainImageUrl = "https://images.unsplash.com/photo-1618366712010-f4ae9c647dcb?w=600",
                IsPublished = true,
                IsFeatured = false,
                CategoryId = headphones.Id,
                VendorId = vendorId,
                BrandId = sony.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "MacBook Pro M3",
                Slug = "macbook-pro-m3",
                SKU = "APPLE-MBP-M3",
                Price = 199999,
                ComparePrice = 219999,
                ShortDescription = "Supercharged for pros",
                LongDescription = "The MacBook Pro with M3 chip delivers exceptional performance for demanding workflows. With up to 12-core CPU and 18-core GPU, it blazes through complex tasks. The stunning Liquid Retina XDR display provides reference-grade color accuracy. All-day battery life and a comprehensive port selection make it the ultimate professional laptop.",
                StockQuantity = 20,
                MainImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600",
                IsPublished = true,
                IsFeatured = true,
                CategoryId = laptops.Id,
                VendorId = vendorId,
                BrandId = apple.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Samsung 65\" OLED TV",
                Slug = "samsung-65-oled-tv",
                SKU = "SAMSUNG-65OLED",
                Price = 189999,
                ComparePrice = 229999,
                ShortDescription = "Stunning OLED picture quality",
                LongDescription = "Immerse yourself in pure blacks and vibrant colors with the Samsung 65-inch OLED TV. Featuring self-lit pixels for infinite contrast, Neural Quantum Processor with 4K upscaling, and Dolby Atmos for cinematic sound. The ultra-slim design and Ambient Mode+ transform your living space into an art gallery when not in use.",
                StockQuantity = 15,
                MainImageUrl = "https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=600",
                IsPublished = true,
                IsFeatured = false,
                CategoryId = electronics.Id,
                VendorId = vendorId,
                BrandId = samsung.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Nike Air Max 270",
                Slug = "nike-air-max-270",
                SKU = "NIKE-AM270",
                Price = 12999,
                ComparePrice = 15999,
                ShortDescription = "Iconic comfort meets modern style",
                LongDescription = "The Nike Air Max 270 delivers unrivaled comfort with the tallest Air unit yet. Inspired by two iconic Air Max models, it features a sleek, modern design with a large Max Air unit for all-day cushioning. The lightweight upper and durable rubber outsole provide the perfect combination of style and performance for everyday wear.",
                StockQuantity = 200,
                MainImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600",
                IsPublished = true,
                IsFeatured = false,
                CategoryId = mensFashion.Id,
                VendorId = vendorId,
                BrandId = nike.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Adidas Ultraboost",
                Slug = "adidas-ultraboost",
                SKU = "ADIDAS-UB23",
                Price = 16999,
                ComparePrice = 19999,
                ShortDescription = "Responsive running shoes",
                LongDescription = "The Adidas Ultraboost combines peak performance with street-ready style. BOOST midsole technology returns energy with every step, while the Primeknit+ upper adapts to your foot for a seamless fit. The Continental rubber outsole provides extraordinary grip in all conditions, making these shoes perfect for both running and everyday wear.",
                StockQuantity = 150,
                MainImageUrl = "https://images.unsplash.com/photo-1608231387042-66d1773070a5?w=600",
                IsPublished = true,
                IsFeatured = false,
                CategoryId = mensFashion.Id,
                VendorId = vendorId,
                BrandId = adidas.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Sony PlayStation 5",
                Slug = "sony-playstation-5",
                SKU = "SONY-PS5",
                Price = 49999,
                ComparePrice = 54999,
                ShortDescription = "Next-gen gaming console",
                LongDescription = "Experience a new generation of gaming with the Sony PlayStation 5. The custom SSD delivers lightning-fast load times, while the DualSense controller brings games to life with haptic feedback and adaptive triggers. Support for 4K graphics at 120fps, ray tracing, and a vast library of games make the PS5 the ultimate gaming console.",
                StockQuantity = 25,
                MainImageUrl = "https://images.unsplash.com/photo-1606144042614-b2417e99c4e3?w=600",
                IsPublished = true,
                IsFeatured = true,
                CategoryId = electronics.Id,
                VendorId = vendorId,
                BrandId = sony.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Samsung Galaxy Watch 6",
                Slug = "samsung-galaxy-watch-6",
                SKU = "SAMSUNG-GW6",
                Price = 27999,
                ComparePrice = 32999,
                ShortDescription = "Health-focused smartwatch",
                LongDescription = "The Samsung Galaxy Watch 6 is your ultimate health companion. Track sleep with advanced sleep coaching, monitor heart rate and blood oxygen, and get body composition measurements right from your wrist. The sapphire crystal display is 20% larger with slimmer bezels, and Wear OS powered by Samsung provides seamless Galaxy ecosystem integration.",
                StockQuantity = 75,
                MainImageUrl = "https://images.unsplash.com/photo-1579586337278-3befd40fd17a?w=600",
                IsPublished = true,
                IsFeatured = false,
                CategoryId = electronics.Id,
                VendorId = vendorId,
                BrandId = samsung.Id,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Leather Messenger Bag",
                Slug = "leather-messenger-bag",
                SKU = "FASHION-LMB01",
                Price = 4999,
                ComparePrice = 7999,
                ShortDescription = "Handcrafted genuine leather bag",
                LongDescription = "Elevate your style with this handcrafted genuine leather messenger bag. Made from premium full-grain leather that develops a beautiful patina over time. Features an adjustable shoulder strap, multiple interior compartments for organization, and a padded laptop sleeve. Perfect for professionals who appreciate timeless craftsmanship and functional design.",
                StockQuantity = 100,
                MainImageUrl = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=600",
                IsPublished = true,
                IsFeatured = false,
                CategoryId = womensFashion.Id,
                VendorId = vendorId,
                BrandId = null,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Smart Home Speaker",
                Slug = "smart-home-speaker",
                SKU = "HOME-SHS01",
                Price = 8999,
                ComparePrice = 11999,
                ShortDescription = "Voice-controlled smart speaker",
                LongDescription = "Transform your home with this intelligent voice-controlled speaker. Equipped with a powerful 360-degree sound system, it fills your room with rich, immersive audio. Control smart home devices, play music, set reminders, get news briefings, and much more — all with simple voice commands. Compatible with all major smart home ecosystems.",
                StockQuantity = 60,
                MainImageUrl = "https://images.unsplash.com/photo-1543512214-318c7553f230?w=600",
                IsPublished = true,
                IsFeatured = false,
                CategoryId = homeLiving.Id,
                VendorId = vendorId,
                BrandId = null,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Name = "Wireless Earbuds Pro",
                Slug = "wireless-earbuds-pro",
                SKU = "SONY-WF1000XM5",
                Price = 19999,
                ComparePrice = 24999,
                ShortDescription = "Premium true wireless earbuds",
                LongDescription = "Experience truly wireless freedom with these premium earbuds featuring industry-leading noise cancellation. The compact design houses powerful 8.4mm drivers for exceptional sound quality. Adaptive Sound Control automatically adjusts settings based on your activity. With up to 24 hours of total battery life and IPX4 water resistance, these earbuds are built for every moment.",
                StockQuantity = 80,
                MainImageUrl = "https://images.unsplash.com/photo-1590658268037-6bf12f032f55?w=600",
                IsPublished = true,
                IsFeatured = false,
                CategoryId = headphones.Id,
                VendorId = vendorId,
                BrandId = sony.Id,
                CreatedAt = now,
                IsActive = true
            }
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedCmsAsync(CmsDbContext db, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var pages = new List<CmsPage>
        {
            new()
            {
                Title = "About Us",
                Slug = "about-us",
                Content = """
                    <h1>About CommerceHub</h1>
                    <p>CommerceHub is a modern multi-vendor e-commerce platform designed to connect sellers with buyers worldwide. Our mission is to empower businesses of all sizes with the tools they need to succeed in the digital marketplace.</p>
                    <h2>Our Story</h2>
                    <p>Founded with the vision of creating a seamless marketplace experience, CommerceHub brings together vendors and customers through a reliable, secure, and feature-rich platform. We believe in fostering a community where quality products meet discerning customers.</p>
                    <h2>Our Mission</h2>
                    <p>To provide a world-class multi-vendor marketplace that enables businesses to reach global audiences while delivering exceptional shopping experiences to customers everywhere.</p>
                    <h2>Why Choose Us</h2>
                    <ul>
                        <li>Multi-vendor marketplace with diverse product offerings</li>
                        <li>Secure payment processing and buyer protection</li>
                        <li>Real-time order tracking and notifications</li>
                        <li>AI-powered product recommendations</li>
                        <li>24/7 customer support</li>
                    </ul>
                    """,
                MetaTitle = "About Us - CommerceHub",
                MetaDescription = "Learn about CommerceHub, a modern multi-vendor e-commerce platform connecting sellers with buyers worldwide.",
                IsPublished = true,
                PublishedAt = now,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Title = "Contact Us",
                Slug = "contact-us",
                Content = """
                    <h1>Contact Us</h1>
                    <p>We'd love to hear from you. Here's how you can reach us:</p>
                    <h2>Customer Support</h2>
                    <p><strong>Email:</strong> support@commercehub.com</p>
                    <p><strong>Phone:</strong> +1-555-0100</p>
                    <p><strong>Hours:</strong> Monday - Friday, 9:00 AM - 6:00 PM (EST)</p>
                    <h2>Business Inquiries</h2>
                    <p><strong>Email:</strong> business@commercehub.com</p>
                    <p><strong>Phone:</strong> +1-555-0200</p>
                    <h2>Vendor Support</h2>
                    <p><strong>Email:</strong> vendors@commercehub.com</p>
                    <p><strong>Phone:</strong> +1-555-0300</p>
                    <h2>Office Address</h2>
                    <p>CommerceHub Inc.<br>123 Commerce Street<br>San Francisco, CA 94105<br>United States</p>
                    """,
                MetaTitle = "Contact Us - CommerceHub",
                MetaDescription = "Get in touch with CommerceHub. Reach our customer support, business team, or vendor support.",
                IsPublished = true,
                PublishedAt = now,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Title = "Privacy Policy",
                Slug = "privacy-policy",
                Content = """
                    <h1>Privacy Policy</h1>
                    <p><em>Last updated: January 1, 2024</em></p>
                    <h2>Introduction</h2>
                    <p>Welcome to CommerceHub. We are committed to protecting your personal information and your right to privacy. This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you use our platform.</p>
                    <h2>Information We Collect</h2>
                    <p>We collect information you provide directly, including:</p>
                    <ul>
                        <li>Account information (name, email, phone number)</li>
                        <li>Payment information (processed securely through our payment partners)</li>
                        <li>Shipping addresses</li>
                        <li>Product reviews and ratings</li>
                        <li>Communication with customer support</li>
                    </ul>
                    <h2>How We Use Your Information</h2>
                    <ul>
                        <li>To process transactions and fulfill orders</li>
                        <li>To send order confirmations and updates</li>
                        <li>To provide customer support</li>
                        <li>To personalize your shopping experience</li>
                        <li>To improve our platform and services</li>
                        <li>To send promotional communications (with your consent)</li>
                    </ul>
                    <h2>Data Security</h2>
                    <p>We implement industry-standard security measures to protect your personal information. All payment data is encrypted and processed through PCI-compliant payment processors.</p>
                    <h2>Contact Us</h2>
                    <p>If you have questions about this Privacy Policy, please contact us at privacy@commercehub.com.</p>
                    """,
                MetaTitle = "Privacy Policy - CommerceHub",
                MetaDescription = "Learn about how CommerceHub collects, uses, and protects your personal information.",
                IsPublished = true,
                PublishedAt = now,
                CreatedAt = now,
                IsActive = true
            },
            new()
            {
                Title = "Terms of Service",
                Slug = "terms-of-service",
                Content = """
                    <h1>Terms of Service</h1>
                    <p><em>Last updated: January 1, 2024</em></p>
                    <h2>Acceptance of Terms</h2>
                    <p>By accessing or using CommerceHub, you agree to be bound by these Terms of Service. If you do not agree to these terms, please do not use our platform.</p>
                    <h2>Account Registration</h2>
                    <p>You must be at least 18 years old to create an account. You are responsible for maintaining the confidentiality of your account credentials and for all activities under your account.</p>
                    <h2>Marketplace Rules</h2>
                    <ul>
                        <li>All products listed must comply with applicable laws and regulations</li>
                        <li>Vendors must provide accurate product descriptions and images</li>
                        <li>Prohibited items include counterfeit goods, illegal products, and hazardous materials</li>
                        <li>Vendors must fulfill orders within the specified delivery timeframe</li>
                    </ul>
                    <h2>Payments</h2>
                    <p>All transactions are processed securely. Prices are displayed in the local currency. CommerceHub charges a commission on each sale as outlined in the vendor agreement.</p>
                    <h2>Returns and Refunds</h2>
                    <p>Buyers may request returns within 30 days of delivery for items in original condition. Refunds are processed within 5-10 business days of return approval.</p>
                    <h2>Limitation of Liability</h2>
                    <p>CommerceHub acts as an intermediary between buyers and vendors. We are not liable for product quality, safety, or legality of items listed on the platform.</p>
                    <h2>Contact</h2>
                    <p>For questions about these Terms, contact us at legal@commercehub.com.</p>
                    """,
                MetaTitle = "Terms of Service - CommerceHub",
                MetaDescription = "Read the terms and conditions governing the use of the CommerceHub platform.",
                IsPublished = true,
                PublishedAt = now,
                CreatedAt = now,
                IsActive = true
            }
        };

        db.CmsPages.AddRange(pages);
        await db.SaveChangesAsync(ct);
    }
}

public class SeedResult
{
    public string Message { get; set; } = string.Empty;
    public bool AlreadySeeded { get; set; }
    public bool IdentitySeeded { get; set; }
    public bool VendorSeeded { get; set; }
    public bool ProductsSeeded { get; set; }
    public bool CmsSeeded { get; set; }
    public List<string> Errors { get; set; } = new();
}
