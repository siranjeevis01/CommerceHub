using CommerceHub.Modules.Cart.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Modules.Cart.UnitTests;

public class CartDomainTests
{
    [Fact]
    public void Cart_ShouldInitializeWithEmptyItems()
    {
        var cart = new global::CommerceHub.Modules.Cart.Domain.Models.Cart { Id = "cart:user:1" };
        cart.Items.Should().BeEmpty();
        cart.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CartItem_ShouldCalculateTotalPrice()
    {
        var item = new CartItem
        {
            ProductId = 1,
            Name = "Test Product",
            UnitPrice = 25.00m,
            Quantity = 3
        };

        item.TotalPrice.Should().Be(75.00m);
    }

    [Fact]
    public void CartItem_TotalPrice_ShouldBeZero_WhenQuantityIsZero()
    {
        var item = new CartItem
        {
            ProductId = 1,
            UnitPrice = 50.00m,
            Quantity = 0
        };

        item.TotalPrice.Should().Be(0);
    }

    [Fact]
    public void Cart_ShouldUpdateTimestamp()
    {
        var cart = new global::CommerceHub.Modules.Cart.Domain.Models.Cart { Id = "cart:session:abc" };
        var before = cart.UpdatedAt;
        cart.UpdatedAt = DateTime.UtcNow;
        cart.UpdatedAt.Should().NotBe(before);
    }
}
