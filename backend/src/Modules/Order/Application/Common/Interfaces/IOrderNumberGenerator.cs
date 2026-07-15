namespace CommerceHub.Modules.Order.Application.Common.Interfaces;

public interface IOrderNumberGenerator
{
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
}
