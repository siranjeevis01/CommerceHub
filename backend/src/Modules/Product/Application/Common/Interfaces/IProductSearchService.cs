using CommerceHub.Modules.Product.Domain.Entities;

namespace CommerceHub.Modules.Product.Application.Common.Interfaces;

public interface IProductSearchService
{
    Task IndexProductAsync(CommerceHub.Modules.Product.Domain.Entities.Product product, CancellationToken cancellationToken = default);
    Task RemoveProductIndexAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
}
