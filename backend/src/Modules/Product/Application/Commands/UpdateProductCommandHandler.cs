using AutoMapper;
using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Product.Application.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;
    private readonly IProductSearchService _searchService;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateProductCommandHandler(
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

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product is null)
            throw new KeyNotFoundException($"Product with Id {request.Id} was not found.");

        _mapper.Map(request, product);
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new ProductUpdated
        {
            ProductId = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            SKU = product.SKU,
            Price = product.Price,
            CategoryId = product.CategoryId,
            VendorId = product.VendorId,
            UpdatedAt = product.UpdatedAt ?? DateTime.UtcNow
        }, cancellationToken);

        await _searchService.IndexProductAsync(product, cancellationToken);
    }
}
