using AutoMapper;
using CommerceHub.Modules.Ai.Application.DTOs;
using CommerceHub.Modules.Ai.Domain.Entities;

namespace CommerceHub.Modules.Ai.Application.Mappings;

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
