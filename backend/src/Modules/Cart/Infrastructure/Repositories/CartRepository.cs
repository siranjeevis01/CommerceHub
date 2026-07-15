using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using CommerceHub.Modules.Cart.Application.Common.Interfaces;
using CartModel = CommerceHub.Modules.Cart.Domain.Models.Cart;

namespace CommerceHub.Modules.Cart.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IDatabase _redis;
    private readonly ILogger<CartRepository> _logger;

    public CartRepository(IConnectionMultiplexer redis, ILogger<CartRepository> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<CartModel?> GetCartAsync(string cartKey, CancellationToken cancellationToken = default)
    {
        var data = await _redis.StringGetAsync(new RedisKey(cartKey));
        if (!data.HasValue)
            return null;

        return JsonSerializer.Deserialize<CartModel>((string)data!);
    }

    public async Task SaveCartAsync(string cartKey, CartModel cart, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        expiry ??= TimeSpan.FromDays(7);
        await _redis.StringSetAsync(new RedisKey(cartKey), JsonSerializer.Serialize(cart), expiry);
        _logger.LogDebug("Cart saved: {CartKey}", cartKey);
    }

    public async Task DeleteCartAsync(string cartKey, CancellationToken cancellationToken = default)
    {
        await _redis.KeyDeleteAsync(new RedisKey(cartKey));
        _logger.LogDebug("Cart deleted: {CartKey}", cartKey);
    }

    public async Task<bool> ExistsAsync(string cartKey, CancellationToken cancellationToken = default)
    {
        return await _redis.KeyExistsAsync(new RedisKey(cartKey));
    }
}
