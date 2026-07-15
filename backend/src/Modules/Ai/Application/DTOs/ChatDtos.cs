namespace CommerceHub.Modules.Ai.Application.DTOs;

public class ChatRequestDto
{
    public string Message { get; set; } = string.Empty;
    public int? ConversationId { get; set; }
    public string? Context { get; set; }
}

public class ChatResponseDto
{
    public int ConversationId { get; set; }
    public string Reply { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public string? Action { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public List<ProductRecommendationDto>? Recommendations { get; set; }
    public List<ProductSearchResultDto>? SearchResults { get; set; }
}

public class ConversationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public int MessageCount { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductRecommendationDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public double Score { get; set; }
    public string? Reason { get; set; }
}

public class ProductSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public string? MainImageUrl { get; set; }
    public string? CategoryName { get; set; }
    public string? VendorName { get; set; }
    public double? RelevanceScore { get; set; }
}

public class SearchResultDto
{
    public List<ProductSearchResultDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? CorrectedQuery { get; set; }
    public string? Intent { get; set; }
}

public class SearchFiltersDto
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? VendorIds { get; set; }
    public List<int>? BrandIds { get; set; }
    public bool? InStock { get; set; }
    public bool? IsFeatured { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public double? MinRating { get; set; }
}
