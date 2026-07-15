using CommerceHub.Shared.Kernel.Entities;

namespace CommerceHub.Modules.Ai.Domain.Entities;

public class Conversation : AggregateRoot
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? Metadata { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class Message : BaseEntity
{
    public int ConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public string? Confidence { get; set; }
    public string? Source { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Conversation Conversation { get; set; } = null!;
}

public class ProductRecommendation : BaseEntity
{
    public int UserId { get; set; }
    public string RecommendationType { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string? Reason { get; set; }
    public bool IsViewed { get; set; }
    public bool IsClicked { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class SearchQuery : BaseEntity
{
    public int? UserId { get; set; }
    public string RawQuery { get; set; } = string.Empty;
    public string? ParsedIntent { get; set; }
    public string? ParsedEntities { get; set; }
    public string? CorrectedQuery { get; set; }
    public string? SearchResults { get; set; }
    public long ResponseTimeMs { get; set; }
    public bool IsSuccessful { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

public class AISession : BaseEntity
{
    public int UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string? Preferences { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActivityAt { get; set; }
}
