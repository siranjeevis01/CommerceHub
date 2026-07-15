using CommerceHub.Shared.Kernel.Entities;

namespace CommerceHub.Shared.Kernel.Interfaces;

public interface IRepository<T, TId>
    where T : BaseEntity<TId>
    where TId : notnull
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}

public interface IRepository<T> : IRepository<T, int>
    where T : BaseEntity<int>
{
}
