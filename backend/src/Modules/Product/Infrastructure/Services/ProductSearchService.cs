using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Modules.Product.Infrastructure.Data;
using ProductEntity = CommerceHub.Modules.Product.Domain.Entities.Product;

namespace CommerceHub.Modules.Product.Infrastructure.Services;

public class ProductSearchService : IProductSearchService
{
    private readonly ProductDbContext _dbContext;
    private readonly Meilisearch.MeilisearchClient? _client;
    private readonly ILogger<ProductSearchService> _logger;

    public ProductSearchService(ProductDbContext dbContext, ILogger<ProductSearchService> logger, Meilisearch.MeilisearchClient? client = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _client = client;
    }

    public async Task IndexProductAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        if (_client is null) return;

        try
        {
            var doc = new ProductIndexDocument
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                SKU = product.SKU,
                Price = product.Price,
                CategoryId = product.CategoryId,
                VendorId = product.VendorId,
                BrandId = product.BrandId,
                Description = product.ShortDescription
            };

            var index = _client.Index("products");
            await index.AddDocumentsAsync(new[] { doc });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Meilisearch indexing failed for product {ProductId} - search index may be stale", product.Id);
        }
    }

    public async Task RemoveProductIndexAsync(int productId, CancellationToken cancellationToken = default)
    {
        if (_client is null) return;

        try
        {
            var index = _client.Index("products");
            await index.DeleteOneDocumentAsync(productId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Meilisearch index removal failed for product {ProductId} - search index may be stale", productId);
        }
    }

    public async Task<IReadOnlyList<int>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (_client is null)
            return await SearchWithMySqlAsync(query, page, pageSize, cancellationToken);

        try
        {
            var index = _client.Index("products");
            var results = await index.SearchAsync<ProductIndexDocument>(query, new Meilisearch.SearchQuery
            {
                Limit = pageSize,
                Offset = (page - 1) * pageSize
            });

            return results.Hits.Select(h => h.Id).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Meilisearch query failed, falling back to MySQL search");
            return await SearchWithMySqlAsync(query, page, pageSize, cancellationToken);
        }
    }

    private async Task<IReadOnlyList<int>> SearchWithMySqlAsync(string query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var lowerQuery = query.ToLower();
        return await _dbContext.Products
            .Where(p => !p.IsDeleted && (
                p.Name.ToLower().Contains(lowerQuery) ||
                p.SKU.ToLower().Contains(lowerQuery) ||
                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(lowerQuery)) ||
                (p.Slug != null && p.Slug.ToLower().Contains(lowerQuery))
            ))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
    }
}

public class ProductIndexDocument
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int VendorId { get; set; }
    public int? BrandId { get; set; }
    public string? Description { get; set; }
}
