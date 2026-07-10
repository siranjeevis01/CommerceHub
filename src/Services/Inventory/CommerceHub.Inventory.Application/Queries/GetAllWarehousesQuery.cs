using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Inventory.Application.Common.Interfaces;
using CommerceHub.Inventory.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Inventory.Application.Queries;

public record GetAllWarehousesQuery : IRequest<List<WarehouseDto>>;

public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, List<WarehouseDto>>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetAllWarehousesQueryHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Warehouses
            .ProjectTo<WarehouseDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
