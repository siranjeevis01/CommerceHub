using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Ai.Application.Common.Interfaces;
using CommerceHub.Modules.Ai.Application.DTOs;
using CommerceHub.Modules.Ai.Domain.Entities;
using CommerceHub.Modules.Ai.Domain.Events;

namespace CommerceHub.Modules.Ai.Application.Commands.Chat;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatResponseDto>
{
    private readonly IAIAgentDbContext _context;
    private readonly ILLMService _llmService;
    private readonly IProductSearchService _searchService;
    private readonly IRecommendationService _recommendationService;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public SendMessageCommandHandler(
        IAIAgentDbContext context,
        ILLMService llmService,
        IProductSearchService searchService,
        IRecommendationService recommendationService,
        IMapper mapper,
        IMediator mediator)
    {
        _context = context;
        _llmService = llmService;
        _searchService = searchService;
        _recommendationService = recommendationService;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<ChatResponseDto> Handle(SendMessageCommand request, CancellationToken ct)
    {
        Conversation conversation;
        if (request.ConversationId.HasValue)
        {
            conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value && c.UserId == request.UserId, ct)
                ?? throw new InvalidOperationException("Conversation not found");
        }
        else
        {
            var intent = await _llmService.DetectIntentAsync(request.Message, ct);
            conversation = new Conversation
            {
                UserId = request.UserId,
                Title = $"Chat about {intent}",
                Status = "Active"
            };
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync(ct);
            await _mediator.Publish(new ConversationStarted
            {
                ConversationId = conversation.Id,
                UserId = request.UserId,
                Title = conversation.Title,
                StartedAt = conversation.CreatedAt
            }, ct);
        }

        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = "User",
            Content = request.Message
        };
        _context.Messages.Add(userMessage);
        await _context.SaveChangesAsync(ct);

        var intentDetected = await _llmService.DetectIntentAsync(request.Message, ct);

        await _mediator.Publish(new MessageReceived
        {
            MessageId = userMessage.Id,
            ConversationId = conversation.Id,
            UserId = request.UserId,
            Content = request.Message,
            Intent = intentDetected,
            ReceivedAt = userMessage.CreatedAt
        }, ct);

        var contextHistory = string.Join("\n", conversation.Messages
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .Select(m => $"{m.Role}: {m.Content}"));

        var response = new ChatResponseDto
        {
            ConversationId = conversation.Id
        };

        if (intentDetected.Contains("search", StringComparison.OrdinalIgnoreCase) ||
            intentDetected.Contains("find", StringComparison.OrdinalIgnoreCase) ||
            intentDetected.Contains("looking for", StringComparison.OrdinalIgnoreCase))
        {
            var searchResult = await _searchService.NaturalLanguageSearchAsync(request.Message, ct: ct);
            response.SearchResults = searchResult.Items;
            response.Intent = "search";
            response.Reply = searchResult.Items.Count > 0
                ? $"I found {searchResult.TotalCount} products matching your request. Here are the top results:"
                : "I couldn't find any products matching your description. Could you try different keywords?";
        }
        else if (intentDetected.Contains("recommend", StringComparison.OrdinalIgnoreCase) ||
                 intentDetected.Contains("suggest", StringComparison.OrdinalIgnoreCase))
        {
            var recommendations = await _recommendationService.GetPersonalizedRecommendationsAsync(request.UserId, 10, ct);
            response.Recommendations = recommendations;
            response.Intent = "recommendation";
            response.Reply = recommendations.Count > 0
                ? "Based on your preferences, here are my recommendations for you:"
                : "I don't have enough data to make personalized recommendations yet. Try browsing some products first!";
        }
        else
        {
            var llmReply = await _llmService.ChatAsync(request.Message, contextHistory, ct);
            response.Intent = intentDetected;
            response.Reply = llmReply;
        }

        response.Data = new Dictionary<string, object>
        {
            ["intent"] = intentDetected,
            ["conversation_id"] = conversation.Id
        };

        var aiMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = "AI",
            Content = response.Reply,
            Intent = intentDetected
        };
        _context.Messages.Add(aiMessage);

        conversation.LastActivityAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return response;
    }
}
