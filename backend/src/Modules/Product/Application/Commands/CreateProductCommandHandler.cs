using AutoMapper;
using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using MediatR;
using ProductEntity = CommerceHub.Modules.Product.Domain.Entities.Product;
using ProductVariantEntity = CommerceHub.Modules.Product.Domain.Entities.ProductVariant;

namespace CommerceHub.Modules.Product.Application.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;
    private readonly IProductSearchService _searchService;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateProductCommandHandler(
        IProductDbContext context,
        IMapper mapper,
        IProductSearchService searchService,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _searchService = searchService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = _mapper.Map<ProductEntity>(request);

        product.Slug = GenerateSlug(request.Name);

        if (request.Variants is { Count: > 0 })
        {
            foreach (var variantDto in request.Variants)
            {
                var variant = _mapper.Map<ProductVariantEntity>(variantDto);
                product.Variants.Add(variant);
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new ProductCreated
        {
            ProductId = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            SKU = product.SKU,
            Price = product.Price,
            CategoryId = product.CategoryId,
            VendorId = product.VendorId,
            CreatedAt = product.CreatedAt
        }, cancellationToken);

        await _searchService.IndexProductAsync(product, cancellationToken);

        return product.Id;
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-");

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = slug.Trim('-');

        return slug + "-" + Guid.NewGuid().ToString("N")[..8];
    }
}
