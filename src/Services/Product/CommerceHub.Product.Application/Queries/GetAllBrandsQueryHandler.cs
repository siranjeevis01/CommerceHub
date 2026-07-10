using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Product.Application.Common.Interfaces;
using CommerceHub.Product.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Queries;

public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, IReadOnlyList<BrandDto>>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;

    public GetAllBrandsQueryHandler(IProductDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BrandDto>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Brands
            .AsNoTracking()
            .Include(b => b.Products)
            .Where(b => !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ProjectTo<BrandDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
