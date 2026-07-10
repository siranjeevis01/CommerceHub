namespace CommerceHub.Shared.Contracts.Events;

public record AIQueryReceived
{
    public Guid CorrelationId { get; init; }
    public int? UserId { get; init; }
    public string Query { get; init; } = string.Empty;
    public string Intent { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public DateTime ReceivedAt { get; init; }
}

public record AIResponseGenerated
{
    public Guid CorrelationId { get; init; }
    public int? UserId { get; init; }
    public string Query { get; init; } = string.Empty;
    public string Response { get; init; } = string.Empty;
    public long ResponseTimeMs { get; init; }
    public string Model { get; init; } = string.Empty;
}

public record RecommendationRequested
{
    public int UserId { get; init; }
    public string Context { get; init; } = string.Empty;
    public int Count { get; init; }
    public DateTime RequestedAt { get; init; }
}

public record SearchProcessed
{
    public Guid CorrelationId { get; init; }
    public int? UserId { get; init; }
    public string RawQuery { get; init; } = string.Empty;
    public string CorrectedQuery { get; init; } = string.Empty;
    public string Intent { get; init; } = string.Empty;
    public int ResultsCount { get; init; }
    public long ProcessingTimeMs { get; init; }
}