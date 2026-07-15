using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CommerceHub.Modules.Ai.Application.Common.Interfaces;
using CommerceHub.Modules.Ai.Application.DTOs;
using CommerceHub.Modules.Ai.Domain.Entities;

namespace CommerceHub.Modules.Ai.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IAIAgentDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILLMService _llmService;
    private readonly HttpClient _httpClient;

    public RecommendationService(IAIAgentDbContext context, IDistributedCache cache, ILLMService llmService, HttpClient httpClient)
    {
        _context = context;
        _cache = cache;
        _llmService = llmService;
        _httpClient = httpClient;
    }

    public async Task<List<ProductRecommendationDto>> GetPersonalizedRecommendationsAsync(int userId, int count = 10, CancellationToken ct = default)
    {
        var cacheKey = $"recs_{userId}_{count}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<List<ProductRecommendationDto>>(cached) ?? new();

        var recentRecs = await _context.ProductRecommendations
            .Where(r => r.UserId == userId && !r.IsViewed && r.GeneratedAt > DateTime.UtcNow.AddDays(-7))
            .OrderByDescending(r => r.Score)
            .Take(count)
            .ToListAsync(ct);

        if (recentRecs.Count >= count)
            return recentRecs.Select(r => new ProductRecommendationDto
            {
                ProductId = r.ProductId,
                ProductName = r.ProductName,
                Score = (double)r.Score,
                Reason = r.Reason
            }).ToList();

        var trendingProducts = await GetTrendingProductsAsync(6, ct);
        var result = trendingProducts.Take(count).ToList();

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        }, ct);

        return result;
    }

    public async Task<List<ProductRecommendationDto>> GetSimilarProductsAsync(int productId, int count = 6, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/products/{productId}/similar?count={count}", ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<List<ProductRecommendationDto>>(ct);
                return json ?? new();
            }
        }
        catch { }
        return new();
    }

    public async Task<List<ProductRecommendationDto>> GetTrendingProductsAsync(int count = 10, CancellationToken ct = default)
    {
        var cacheKey = "trending_products";
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<List<ProductRecommendationDto>>(cached) ?? new();

        var trending = await _context.ProductRecommendations
            .Where(r => r.RecommendationType == "trending" && r.GeneratedAt > DateTime.UtcNow.AddDays(-1))
            .OrderByDescending(r => r.Score)
            .Take(count)
            .ToListAsync(ct);

        var result = trending.Select(r => new ProductRecommendationDto
        {
            ProductId = r.ProductId,
            ProductName = r.ProductName,
            Score = (double)r.Score,
            Reason = r.Reason
        }).ToList();

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
        }, ct);

        return result;
    }

    public async Task<List<ProductRecommendationDto>> GetFrequentlyBoughtTogetherAsync(int productId, int count = 4, CancellationToken ct = default)
    {
        var cacheKey = $"bought_together_{productId}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<List<ProductRecommendationDto>>(cached) ?? new();

        try
        {
            var userIds = await _context.ProductRecommendations
                .Where(r => r.ProductId == productId && r.RecommendationType == "purchased")
                .Select(r => r.UserId)
                .Distinct()
                .ToListAsync(ct);

            if (userIds.Count == 0)
                return await GetTrendingProductsAsync(count, ct);

            var coPurchases = await _context.ProductRecommendations
                .Where(r => userIds.Contains(r.UserId)
                    && r.ProductId != productId
                    && r.RecommendationType == "purchased")
                .Select(r => new { r.ProductId, r.ProductName, r.UserId })
                .ToListAsync(ct);

            if (coPurchases.Count == 0)
                return await GetTrendingProductsAsync(count, ct);

            var result = coPurchases
                .GroupBy(r => r.ProductId)
                .Select(g => new ProductRecommendationDto
                {
                    ProductId = g.Key,
                    ProductName = g.Select(x => x.ProductName).First(),
                    Score = g.Select(x => x.UserId).Distinct().Count(),
                    Reason = $"Bought together by {g.Select(x => x.UserId).Distinct().Count()} customers"
                })
                .OrderByDescending(r => r.Score)
                .Take(count)
                .ToList();

            if (result.Count == 0)
                return await GetTrendingProductsAsync(count, ct);

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            }, ct);

            return result;
        }
        catch
        {
            return await GetTrendingProductsAsync(count, ct);
        }
    }

    public async Task RecordInteractionAsync(int userId, int productId, string interactionType, CancellationToken ct = default)
    {
        var recommendation = new ProductRecommendation
        {
            UserId = userId,
            ProductId = productId,
            RecommendationType = interactionType switch
            {
                "view" => "viewed",
                "purchase" => "purchased",
                "cart" => "added_to_cart",
                _ => interactionType
            },
            Score = interactionType switch
            {
                "purchase" => 100,
                "cart" => 50,
                "view" => 10,
                _ => 1
            },
            GeneratedAt = DateTime.UtcNow
        };
        _context.ProductRecommendations.Add(recommendation);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveAsync($"recs_{userId}_10", ct);
        await _cache.RemoveAsync("trending_products", ct);
    }
}
