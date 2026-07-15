using CommerceHub.Modules.Ai.Application.DTOs;

namespace CommerceHub.Modules.Ai.Application.Common.Interfaces;

public interface IRecommendationService
{
    Task<List<ProductRecommendationDto>> GetPersonalizedRecommendationsAsync(int userId, int count = 10, CancellationToken ct = default);
    Task<List<ProductRecommendationDto>> GetSimilarProductsAsync(int productId, int count = 6, CancellationToken ct = default);
    Task<List<ProductRecommendationDto>> GetTrendingProductsAsync(int count = 10, CancellationToken ct = default);
    Task<List<ProductRecommendationDto>> GetFrequentlyBoughtTogetherAsync(int productId, int count = 4, CancellationToken ct = default);
    Task RecordInteractionAsync(int userId, int productId, string interactionType, CancellationToken ct = default);
}
