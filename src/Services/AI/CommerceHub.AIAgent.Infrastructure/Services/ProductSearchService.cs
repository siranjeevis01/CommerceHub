using System.Net.Http.Json;
using System.Text.Json;
using CommerceHub.AIAgent.Application.Common.Interfaces;
using CommerceHub.AIAgent.Application.DTOs;

namespace CommerceHub.AIAgent.Infrastructure.Services;

public class ProductSearchService : IProductSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILLMService _llmService;

    public ProductSearchService(HttpClient httpClient, ILLMService llmService)
    {
        _httpClient = httpClient;
        _llmService = llmService;
    }

    public async Task<SearchResultDto> SearchAsync(string query, SearchFiltersDto? filters = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            var url = $"api/v1/products/search?q={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}";
            if (filters != null)
            {
                if (filters.MinPrice.HasValue) url += $"&minPrice={filters.MinPrice}";
                if (filters.MaxPrice.HasValue) url += $"&maxPrice={filters.MaxPrice}";
                if (filters.InStock.HasValue) url += $"&inStock={filters.InStock}";
                if (!string.IsNullOrEmpty(filters.SortBy)) url += $"&sortBy={filters.SortBy}";
                if (!string.IsNullOrEmpty(filters.SortOrder)) url += $"&sortOrder={filters.SortOrder}";
            }

            var response = await _httpClient.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<SearchResultDto>(ct);
                return json ?? new SearchResultDto();
            }
        }
        catch { }
        return new SearchResultDto();
    }

    public async Task<SearchResultDto> NaturalLanguageSearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var corrected = await _llmService.CorrectQueryAsync(query, ct);
        var entities = await _llmService.ExtractEntitiesAsync(query, ct);
        var intent = await _llmService.DetectIntentAsync(query, ct);

        var searchTerms = new List<string>();
        searchTerms.Add(corrected);
        if (entities.Count > 0)
            searchTerms.AddRange(entities);

        var result = await SearchAsync(string.Join(" ", searchTerms.Distinct()), null, page, pageSize, ct);
        result.CorrectedQuery = corrected;
        result.Intent = intent;
        return result;
    }

    public async Task<List<string>> GetSuggestionsAsync(string partial, int count = 5, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/products/suggestions?q={Uri.EscapeDataString(partial)}&count={count}", ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<List<string>>(ct);
                return json ?? new();
            }
        }
        catch { }
        return new();
    }
}