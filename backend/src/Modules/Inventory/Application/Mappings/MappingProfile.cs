using AutoMapper;
using CommerceHub.Modules.Inventory.Application.DTOs;
using CommerceHub.Modules.Inventory.Domain.Entities;

namespace CommerceHub.Modules.Inventory.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Domain.Entities.Inventory, StockLevelDto>()
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.AvailableQuantity, o => o.MapFrom(s => s.AvailableQuantity));

        CreateMap<InventoryTransaction, StockMovementDto>();

        CreateMap<Warehouse, WarehouseDto>();
    }
}
