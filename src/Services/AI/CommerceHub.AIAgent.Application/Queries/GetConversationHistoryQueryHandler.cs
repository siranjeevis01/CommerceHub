using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.AIAgent.Application.Common.Interfaces;
using CommerceHub.AIAgent.Application.DTOs;
using CommerceHub.AIAgent.Application.Commands.Chat;

namespace CommerceHub.AIAgent.Application.Queries;

public class GetConversationHistoryQueryHandler : IRequestHandler<GetConversationHistoryQuery, List<MessageDto>>
{
    private readonly IAIAgentDbContext _context;
    private readonly IMapper _mapper;

    public GetConversationHistoryQueryHandler(IAIAgentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MessageDto>> Handle(GetConversationHistoryQuery request, CancellationToken ct)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        return _mapper.Map<List<MessageDto>>(messages);
    }
}

public class GetUserConversationsQueryHandler : IRequestHandler<GetUserConversationsQuery, List<ConversationDto>>
{
    private readonly IAIAgentDbContext _context;
    private readonly IMapper _mapper;

    public GetUserConversationsQueryHandler(IAIAgentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ConversationDto>> Handle(GetUserConversationsQuery request, CancellationToken ct)
    {
        var conversations = await _context.Conversations
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.LastActivityAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return _mapper.Map<List<ConversationDto>>(conversations);
    }
}