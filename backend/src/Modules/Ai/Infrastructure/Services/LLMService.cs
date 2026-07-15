using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using CommerceHub.Modules.Ai.Application.Common.Interfaces;
using CommerceHub.Modules.Ai.Application.Services;

#pragma warning disable CA2024

namespace CommerceHub.Modules.Ai.Infrastructure.Services;

public class LLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _cache;
    private readonly string _geminiApiKey;
    private const string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private const string GeminiStreamEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:streamGenerateContent";

    public LLMService(HttpClient httpClient, IDistributedCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
        _geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
    }

    public async Task<string> ChatAsync(string message, string? context = null, CancellationToken ct = default)
    {
        var prompt = BuildChatPrompt(message, context);
        return await CallGeminiAsync(prompt, ct);
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(string message, string? context = null, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var prompt = BuildChatPrompt(message, context);
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"{GeminiStreamEndpoint}?key={_geminiApiKey}";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null && !ct.IsCancellationRequested)
        {
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: ")) continue;
            var data = line[6..];
            if (data == "[DONE]") break;

            string? text = null;
            try
            {
                using var doc = JsonDocument.Parse(data);
                text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch { }

            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }

    public async Task<string> DetectIntentAsync(string message, CancellationToken ct = default)
    {
        foreach (var kvp in IntentPatterns.Patterns)
        {
            if (kvp.Value.Any(p => message.Contains(p, StringComparison.OrdinalIgnoreCase)))
                return kvp.Key;
        }
        try
        {
            var prompt = $"Classify this e-commerce query into one category: search, recommendation, order, product_detail, cart, help, complaint, pricing, account, seller. Query: \"{message}\".\nRespond with only the category name.";
            return await CallGeminiAsync(prompt, ct);
        }
        catch
        {
            return "general";
        }
    }

    public async Task<List<string>> ExtractEntitiesAsync(string message, CancellationToken ct = default)
    {
        try
        {
            var prompt = $"Extract product names, categories, brands, prices, colors from this e-commerce search: \"{message}\". Return as comma-separated list.";
            var result = await CallGeminiAsync(prompt, ct);
            return result.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
        catch
        {
            return new List<string> { message };
        }
    }

    public async Task<string> CorrectQueryAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return query;
        try
        {
            var cacheKey = $"query_correct_{query.GetHashCode()}";
            var cached = await _cache.GetStringAsync(cacheKey, ct);
            if (!string.IsNullOrEmpty(cached)) return cached;

            var prompt = $"Correct spelling and grammar for this e-commerce product search query. Keep it concise. Query: \"{query}\"\nCorrected:";
            var corrected = await CallGeminiAsync(prompt, ct);
            if (!string.IsNullOrWhiteSpace(corrected) && !corrected.Equals(query, StringComparison.OrdinalIgnoreCase))
            {
                await _cache.SetStringAsync(cacheKey, corrected, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                }, ct);
            }
            return corrected;
        }
        catch
        {
            return query;
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent?key={_geminiApiKey}";
            var payload = new { content = new { parts = new[] { new { text } } } };
            var response = await _httpClient.PostAsJsonAsync(url, payload, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            var values = json.GetProperty("embedding").GetProperty("values");
            var embeddings = new List<float>();
            foreach (var v in values.EnumerateArray())
                embeddings.Add(v.GetSingle());
            return embeddings.ToArray();
        }
        catch
        {
            return Array.Empty<float>();
        }
    }

    public async Task<string> AnalyzeSentimentAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var prompt = $"Analyze sentiment of this e-commerce review: \"{text}\". Reply with only: Positive, Negative, or Neutral.";
            return await CallGeminiAsync(prompt, ct);
        }
        catch
        {
            return "Neutral";
        }
    }

    public async Task<List<string>> GenerateRecommendationsAsync(int userId, List<string> preferences, CancellationToken ct = default)
    {
        try
        {
            var prompt = $"Based on these preferences: {string.Join(", ", preferences)}, suggest 5 product categories the user might like. Return as comma-separated list.";
            var result = await CallGeminiAsync(prompt, ct);
            return result.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
        catch
        {
            return preferences;
        }
    }

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_geminiApiKey))
        {
            return await FallbackResponseAsync(prompt);
        }

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 1024,
                topP = 0.95,
                topK = 40
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"{GeminiEndpoint}?key={_geminiApiKey}";
        var response = await _httpClient.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return result.GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }

    private Task<string> FallbackResponseAsync(string message)
    {
        var msg = message.ToLower();
        if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("hey"))
            return Task.FromResult("Hello! I'm CommerceHub AI Assistant. I can help you find products, get recommendations, track orders, and more. What would you like help with?");
        if (msg.Contains("help") || msg.Contains("what can you"))
            return Task.FromResult("I can assist you with:\n- Searching products naturally (\"find red sneakers under $100\")\n- Getting personalized recommendations\n- Answering questions about products\n- Tracking orders\n- Helping with your cart\n- Finding the best deals!\n\nJust type what you need!");
        if (msg.Contains("thank"))
            return Task.FromResult("You're welcome! Is there anything else I can help you with?");
        return Task.FromResult("I understand you're asking about something. Could you please provide more details so I can better assist you? You can ask me about products, orders, recommendations, or any other e-commerce needs.");
    }

    private static string BuildChatPrompt(string message, string? context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are CommerceHub AI, a helpful e-commerce assistant for a multi-vendor marketplace.");
        sb.AppendLine("You help users find products, get recommendations, track orders, and handle account issues.");
        sb.AppendLine("Be concise, friendly, and helpful. If the user asks about something you can't do, politely redirect them.");
        sb.AppendLine("For product searches, suggest using specific keywords and filters.");
        sb.AppendLine("For complaints or issues, empathize and guide them to the return/refund process.");
        sb.AppendLine();
        if (!string.IsNullOrEmpty(context))
        {
            sb.AppendLine("Conversation history:");
            sb.AppendLine(context);
            sb.AppendLine();
        }
        sb.AppendLine($"User: {message}");
        sb.AppendLine("AI:");
        return sb.ToString();
    }
}
