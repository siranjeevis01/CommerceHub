using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceHub.Modules.Inventory.Application.Common.Interfaces;
using CommerceHub.Modules.Inventory.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Inventory.Application.Queries;

public record GetWarehouseByIdQuery(int Id) : IRequest<WarehouseDto?>;

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto?>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public GetWarehouseByIdQueryHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WarehouseDto?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Warehouses
            .ProjectTo<WarehouseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
    }
}
