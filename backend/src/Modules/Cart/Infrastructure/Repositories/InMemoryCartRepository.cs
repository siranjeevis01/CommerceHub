using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using CommerceHub.Modules.Cart.Application.Common.Interfaces;
using CartModel = CommerceHub.Modules.Cart.Domain.Models.Cart;

namespace CommerceHub.Modules.Cart.Infrastructure.Repositories;

public class InMemoryCartRepository : ICartRepository
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<InMemoryCartRepository> _logger;

    public InMemoryCartRepository(IDistributedCache cache, ILogger<InMemoryCartRepository> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<CartModel?> GetCartAsync(string cartKey, CancellationToken cancellationToken = default)
    {
        var data = await _cache.GetAsync(cartKey, cancellationToken);
        if (data == null) return null;
        return JsonSerializer.Deserialize<CartModel>(data);
    }

    public async Task SaveCartAsync(string cartKey, CartModel cart, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiry.HasValue) options.SetAbsoluteExpiration(expiry.Value);
        else options.SetAbsoluteExpiration(TimeSpan.FromDays(7));
        await _cache.SetAsync(cartKey, JsonSerializer.SerializeToUtf8Bytes(cart), options, cancellationToken);
        _logger.LogDebug("Cart saved (in-memory): {CartKey}", cartKey);
    }

    public async Task DeleteCartAsync(string cartKey, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(cartKey, cancellationToken);
        _logger.LogDebug("Cart deleted (in-memory): {CartKey}", cartKey);
    }

    public async Task<bool> ExistsAsync(string cartKey, CancellationToken cancellationToken = default)
    {
        var data = await _cache.GetAsync(cartKey, cancellationToken);
        return data != null;
    }
}
