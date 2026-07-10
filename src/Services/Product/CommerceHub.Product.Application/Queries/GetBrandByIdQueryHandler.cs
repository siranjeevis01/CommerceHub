using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Product.Application.Common.Interfaces;
using CommerceHub.Product.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Product.Application.Queries;

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, BrandDto?>
{
    private readonly IProductDbContext _context;
    private readonly IMapper _mapper;

    public GetBrandByIdQueryHandler(IProductDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BrandDto?> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Brands
            .AsNoTracking()
            .Include(b => b.Products)
            .Where(b => b.Id == request.Id && !b.IsDeleted)
            .ProjectTo<BrandDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
