using CommerceHub.Product.Domain.Entities;

namespace CommerceHub.Product.Application.Common.Interfaces;

public interface IProductSearchService
{
    Task IndexProductAsync(CommerceHub.Product.Domain.Entities.Product product, CancellationToken cancellationToken = default);
    Task RemoveProductIndexAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
}
