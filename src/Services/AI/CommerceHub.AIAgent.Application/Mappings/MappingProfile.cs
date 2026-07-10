using AutoMapper;
using CommerceHub.AIAgent.Application.DTOs;
using CommerceHub.AIAgent.Domain.Entities;

namespace CommerceHub.AIAgent.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Conversation, ConversationDto>()
            .ForMember(d => d.MessageCount, o => o.MapFrom(s => s.Messages.Count));

        CreateMap<Message, MessageDto>();

        CreateMap<ProductRecommendation, ProductRecommendationDto>()
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductName))
            .ForMember(d => d.Score, o => o.MapFrom(s => (double)s.Score))
            .ForMember(d => d.Reason, o => o.MapFrom(s => s.Reason));
    }
}