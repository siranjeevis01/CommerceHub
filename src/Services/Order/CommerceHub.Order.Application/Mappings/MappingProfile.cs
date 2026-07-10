using AutoMapper;
using CommerceHub.Order.Application.DTOs;
using CommerceHub.Order.Application.Queries;
using CommerceHub.Order.Domain.Entities;
using OrderEntity = CommerceHub.Order.Domain.Entities.Order;

namespace CommerceHub.Order.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<OrderEntity, OrderDto>()
            .ForMember(d => d.SubTotal, o => o.MapFrom(s => s.Subtotal))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.ShippingAddress, o => o.Ignore())
            .ForMember(d => d.Notes, o => o.Ignore())
            .ForMember(d => d.ConfirmedAt, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Confirmed")
                    .Select(h => (DateTime?)h.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(d => d.ShippedAt, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Shipped")
                    .Select(h => (DateTime?)h.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(d => d.DeliveredAt, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Delivered")
                    .Select(h => (DateTime?)h.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(d => d.CancelledAt, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Cancelled")
                    .Select(h => (DateTime?)h.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(d => d.CancellationReason, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Cancelled")
                    .Select(h => h.Remarks)
                    .FirstOrDefault()));

        CreateMap<OrderEntity, OrderListDto>()
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount))
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count));

        CreateMap<OrderEntity, OrderStatusDto>()
            .ForMember(d => d.OrderId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.OrderNumber, o => o.MapFrom(s => s.OrderNumber))
            .ForMember(d => d.OrderStatus, o => o.MapFrom(s => s.OrderStatus))
            .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus))
            .ForMember(d => d.ConfirmedAt, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Confirmed")
                    .Select(h => (DateTime?)h.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(d => d.ShippedAt, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Shipped")
                    .Select(h => (DateTime?)h.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(d => d.DeliveredAt, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Delivered")
                    .Select(h => (DateTime?)h.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(d => d.CancelledAt, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Cancelled")
                    .Select(h => (DateTime?)h.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(d => d.CancellationReason, o => o.MapFrom(s =>
                s.StatusHistories
                    .Where(h => h.ToStatus == "Cancelled")
                    .Select(h => h.Remarks)
                    .FirstOrDefault()));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.VariantId, o => o.MapFrom(s => s.ProductVariantId));

        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();
    }
}
