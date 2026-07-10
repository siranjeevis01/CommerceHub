using AutoMapper;
using CommerceHub.Inventory.Application.DTOs;
using CommerceHub.Inventory.Domain.Entities;

namespace CommerceHub.Inventory.Application.Mappings;

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
