namespace CommerceHub.Modules.Ai.Application.Common.Interfaces;

public interface ILLMService
{
    Task<string> ChatAsync(string message, string? context = null, CancellationToken ct = default);
    IAsyncEnumerable<string> ChatStreamAsync(string message, string? context = null, CancellationToken ct = default);
    Task<string> DetectIntentAsync(string message, CancellationToken ct = default);
    Task<List<string>> ExtractEntitiesAsync(string message, CancellationToken ct = default);
    Task<string> CorrectQueryAsync(string query, CancellationToken ct = default);
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
    Task<string> AnalyzeSentimentAsync(string text, CancellationToken ct = default);
    Task<List<string>> GenerateRecommendationsAsync(int userId, List<string> preferences, CancellationToken ct = default);
}

public class IntentResult
{
    public string Intent { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public Dictionary<string, string> Entities { get; set; } = new();
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public string? Action { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}
