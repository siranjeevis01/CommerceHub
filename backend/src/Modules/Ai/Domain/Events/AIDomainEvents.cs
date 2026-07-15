using CommerceHub.Shared.Kernel.Entities;

namespace CommerceHub.Modules.Ai.Domain.Events;

public record ConversationStarted : IDomainEvent
{
    public int ConversationId { get; init; }
    public int UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
}

public record MessageReceived : IDomainEvent
{
    public int MessageId { get; init; }
    public int ConversationId { get; init; }
    public int UserId { get; init; }
    public string Content { get; init; } = string.Empty;
    public string Intent { get; init; } = string.Empty;
    public DateTime ReceivedAt { get; init; }
}

public record RecommendationGenerated : IDomainEvent
{
    public int RecommendationId { get; init; }
    public int UserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public List<int> ProductIds { get; init; } = new();
    public DateTime GeneratedAt { get; init; }
}

public record SearchExecuted : IDomainEvent
{
    public int SearchQueryId { get; init; }
    public int? UserId { get; init; }
    public string RawQuery { get; init; } = string.Empty;
    public string Intent { get; init; } = string.Empty;
    public int ResultCount { get; init; }
    public long ResponseTimeMs { get; init; }
}
