using CommerceHub.Modules.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Product.Application.Common.Interfaces;

public interface IProductDbContext
{
    DbSet<CommerceHub.Modules.Product.Domain.Entities.Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<CategoryAttribute> CategoryAttributes { get; }
    DbSet<ProductAttributeValue> ProductAttributeValues { get; }
    DbSet<Review> Reviews { get; }
    DbSet<WishlistItem> WishlistItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
