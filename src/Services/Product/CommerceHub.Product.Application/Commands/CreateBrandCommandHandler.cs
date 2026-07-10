using AutoMapper;
using CommerceHub.Product.Application.Common.Interfaces;
using CommerceHub.Product.Domain.Entities;
using MediatR;

namespace CommerceHub.Product.Application.Commands;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, int>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;

    public CreateBrandCommandHandler(IProductDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = _mapper.Map<Brand>(request);

        _context.Brands.Add(brand);
        await _context.SaveChangesAsync(cancellationToken);

        return brand.Id;
    }
}
