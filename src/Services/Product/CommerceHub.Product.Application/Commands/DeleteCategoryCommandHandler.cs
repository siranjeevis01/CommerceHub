using CommerceHub.Product.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Commands;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IProductDbContext _context;

    public DeleteCategoryCommandHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (category is null)
            throw new KeyNotFoundException($"Category with Id {request.Id} was not found.");

        category.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
