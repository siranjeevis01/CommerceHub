using CommerceHub.AIAgent.Application.DTOs;

namespace CommerceHub.AIAgent.Application.Common.Interfaces;

public interface IProductSearchService
{
    Task<SearchResultDto> SearchAsync(string query, SearchFiltersDto? filters = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<SearchResultDto> NaturalLanguageSearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<List<string>> GetSuggestionsAsync(string partial, int count = 5, CancellationToken ct = default);
}