using CommerceHub.Modules.Product.Application.Common.Interfaces;
using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Product.Application.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductDbContext _context;
    private readonly IProductSearchService _searchService;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeleteProductCommandHandler(
        IProductDbContext context,
        IProductSearchService searchService,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _searchService = searchService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product is null)
            throw new KeyNotFoundException($"Product with Id {request.Id} was not found.");

        product.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new ProductDeleted
        {
            ProductId = product.Id,
            SKU = product.SKU,
            DeletedAt = DateTime.UtcNow
        }, cancellationToken);

        await _searchService.RemoveProductIndexAsync(request.Id, cancellationToken);
    }
}
