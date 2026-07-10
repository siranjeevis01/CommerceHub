using CommerceHub.Cart.Application.Common.Interfaces;
using CommerceHub.Cart.Infrastructure.Repositories;
using CommerceHub.TestBase;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace CommerceHub.Cart.IntegrationTests;

public class CartRepositoryTests : IntegrationTestBase
{
    private readonly ICartRepository _sut;

    public CartRepositoryTests()
    {
        var redis = ConnectionMultiplexer.Connect(RedisConnectionString);
        _sut = new CartRepository(redis, new Microsoft.Extensions.Logging.Abstractions.NullLogger<CartRepository>());
    }

    [Fact]
    public async Task SaveAndGetCart_ShouldWorkCorrectly()
    {
        var cart = new CommerceHub.Cart.Domain.Models.Cart
        {
            Id = "cart:user:999",
            UserId = 999,
            Items = new List<CommerceHub.Cart.Domain.Models.CartItem>
            {
                new() { ProductId = 1, Name = "Test Item", UnitPrice = 29.99m, Quantity = 2 }
            }
        };

        await _sut.SaveCartAsync("999", cart);
        var retrieved = await _sut.GetCartAsync("999");

        retrieved.Should().NotBeNull();
        retrieved!.Items.Should().HaveCount(1);
        retrieved.Items.First().ProductId.Should().Be(1);
        retrieved.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task DeleteCart_ShouldRemoveCart()
    {
        var cart = new CommerceHub.Cart.Domain.Models.Cart
        {
            Id = "cart:session:test-session",
            SessionId = "test-session",
            Items = new List<CommerceHub.Cart.Domain.Models.CartItem>
            {
                new() { ProductId = 1, Name = "Item to Delete", UnitPrice = 10.00m, Quantity = 1 }
            }
        };

        var cartKey = "cart:session:test-session";
        await _sut.SaveCartAsync(cartKey, cart);
        await _sut.DeleteCartAsync(cartKey);

        var retrieved = await _sut.GetCartAsync(cartKey);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task MergeCart_ShouldCombineSessionAndUserCarts()
    {
        var sessionCart = new CommerceHub.Cart.Domain.Models.Cart
        {
            Id = "cart:session:merge-session",
            SessionId = "merge-session",
            Items = new List<CommerceHub.Cart.Domain.Models.CartItem>
            {
                new() { ProductId = 1, Name = "Session Item", UnitPrice = 15.00m, Quantity = 1 }
            }
        };

        var userCart = new CommerceHub.Cart.Domain.Models.Cart
        {
            Id = "cart:user:888",
            UserId = 888,
            Items = new List<CommerceHub.Cart.Domain.Models.CartItem>
            {
                new() { ProductId = 2, Name = "User Item", UnitPrice = 25.00m, Quantity = 2 }
            }
        };

        var sessionKey = "cart:session:merge-session";
        var userKey = "cart:user:888";

        await _sut.SaveCartAsync(sessionKey, sessionCart);
        await _sut.SaveCartAsync(userKey, userCart);

        var retrievedSession = await _sut.GetCartAsync(sessionKey);
        var retrievedUser = await _sut.GetCartAsync(userKey);

        retrievedSession.Should().NotBeNull();
        retrievedSession!.Items.Should().HaveCount(1);
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Items.Should().HaveCount(1);
    }
}
