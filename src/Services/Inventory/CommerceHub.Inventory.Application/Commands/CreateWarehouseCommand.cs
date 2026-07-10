using AutoMapper;
using CommerceHub.Inventory.Application.Common.Interfaces;
using CommerceHub.Inventory.Application.DTOs;
using CommerceHub.Inventory.Domain.Entities;
using MediatR;

namespace CommerceHub.Inventory.Application.Commands;

public record CreateWarehouseCommand(string Name, string? Code, string? Address, string? City, string? Country) : IRequest<WarehouseDto>;

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IInventoryDbContext _context;
    private readonly IMapper _mapper;

    public CreateWarehouseCommandHandler(IInventoryDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse
        {
            Name = request.Name,
            Code = request.Code,
            Address = request.Address,
            City = request.City,
            Country = request.Country
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WarehouseDto>(warehouse);
    }
}
