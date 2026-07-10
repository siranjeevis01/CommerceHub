using AutoMapper;
using CommerceHub.Product.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Commands;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;

    public UpdateBrandCommandHandler(IProductDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.Id && !b.IsDeleted, cancellationToken);

        if (brand is null)
            throw new KeyNotFoundException($"Brand with Id {request.Id} was not found.");

        _mapper.Map(request, brand);
        brand.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
