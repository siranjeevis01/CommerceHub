using MediatR;
using CommerceHub.AIAgent.Application.DTOs;

namespace CommerceHub.AIAgent.Application.Commands.Chat;

public record SendMessageCommand : IRequest<ChatResponseDto>
{
    public int UserId { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? ConversationId { get; init; }
    public string? Context { get; init; }
}

public record CreateConversationCommand : IRequest<ConversationDto>
{
    public int UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? InitialMessage { get; init; }
}

public record GetConversationHistoryQuery : IRequest<List<MessageDto>>
{
    public int ConversationId { get; init; }
    public int UserId { get; init; }
}

public record GetUserConversationsQuery : IRequest<List<ConversationDto>>
{
    public int UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record NaturalLanguageSearchCommand : IRequest<SearchResultDto>
{
    public int? UserId { get; init; }
    public string Query { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record GetRecommendationsQuery : IRequest<List<ProductRecommendationDto>>
{
    public int UserId { get; init; }
    public int Count { get; init; } = 10;
    public string? Type { get; init; }
}

public record RecordInteractionCommand : IRequest<bool>
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
    public string InteractionType { get; init; } = string.Empty;
}