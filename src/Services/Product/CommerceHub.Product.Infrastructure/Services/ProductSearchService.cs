using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using CommerceHub.Product.Application.Common.Interfaces;
using ProductEntity = CommerceHub.Product.Domain.Entities.Product;

namespace CommerceHub.Product.Infrastructure.Services;

public class ProductSearchService : IProductSearchService
{
    private readonly ElasticsearchClient _client;

    public ProductSearchService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task IndexProductAsync(ProductEntity product, CancellationToken cancellationToken = default)
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

        await _client.IndexAsync(doc, idx => idx
            .Index("products")
            .Id(product.Id), cancellationToken);
    }

    public async Task RemoveProductIndexAsync(int productId, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<ProductIndexDocument>(productId, d => d
            .Index("products"), cancellationToken);
    }

    public async Task<IReadOnlyList<int>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var from = (page - 1) * pageSize;

        var response = await _client.SearchAsync<ProductIndexDocument>(s => s
            .Index("products")
            .From(from)
            .Size(pageSize)
            .Query(q => q
                .Bool(b => b
                    .Should(
                        new Action<QueryDescriptor<ProductIndexDocument>>[]
                        {
                            ms => ms.Match(m => m.Field(p => p.Name).Query(query).Boost(2)),
                            ms => ms.Match(m => m.Field(p => p.SKU).Query(query)),
                            ms => ms.Match(m => m.Field(p => p.Description).Query(query))
                        }
                    )
                    .MinimumShouldMatch(1)
                )
            ), cancellationToken);

        if (!response.IsValidResponse)
            return Array.Empty<int>();

        return response.Hits.Select(h => int.Parse(h.Id!)).ToList();
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
