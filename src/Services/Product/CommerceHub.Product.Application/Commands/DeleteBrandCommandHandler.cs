using CommerceHub.Product.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Commands;

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand>
{
    private readonly IProductDbContext _context;

    public DeleteBrandCommandHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.Id && !b.IsDeleted, cancellationToken);

        if (brand is null)
            throw new KeyNotFoundException($"Brand with Id {request.Id} was not found.");

        brand.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
