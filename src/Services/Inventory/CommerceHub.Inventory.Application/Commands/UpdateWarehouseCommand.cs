using AutoMapper;
using CommerceHub.Inventory.Application.Common.Interfaces;
using CommerceHub.Inventory.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Inventory.Application.Commands;

public record UpdateWarehouseCommand(int Id, string Name, string? Code, string? Address, string? City, string? Country) : IRequest<WarehouseDto>;

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public UpdateWarehouseCommandHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse is null)
            throw new KeyNotFoundException($"Warehouse with Id {request.Id} not found.");

        warehouse.Name = request.Name;
        warehouse.Code = request.Code;
        warehouse.Address = request.Address;
        warehouse.City = request.City;
        warehouse.Country = request.Country;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WarehouseDto>(warehouse);
    }
}
