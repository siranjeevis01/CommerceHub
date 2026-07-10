using AutoMapper;
using CommerceHub.Cart.Application.Commands;
using CommerceHub.Cart.Application.Common.Interfaces;
using CommerceHub.Cart.Application.DTOs;
using FluentAssertions;
using Moq;
using Xunit;
using CartModel = CommerceHub.Cart.Domain.Models.Cart;
using CartItemModel = CommerceHub.Cart.Domain.Models.CartItem;

namespace CommerceHub.Cart.UnitTests;

public class RemoveFromCartCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RemoveFromCartCommandHandler _handler;

    public RemoveFromCartCommandHandlerTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new RemoveFromCartCommandHandler(_cartRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveItem_WhenItemExists()
    {
        var cart = new CartModel
        {
            Id = "cart:user:1",
            Items = new List<CartItemModel>
            {
                new() { ProductId = 1, VariantId = null, Name = "Product A", UnitPrice = 25.00m, Quantity = 2 },
                new() { ProductId = 2, VariantId = null, Name = "Product B", UnitPrice = 15.00m, Quantity = 1 }
            }
        };

        _cartRepoMock.Setup(x => x.GetCartAsync("cart:user:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepoMock.Setup(x => x.SaveCartAsync(It.IsAny<string>(), It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RemoveFromCartCommand
        {
            CartKey = "cart:user:1",
            ProductId = 1
        };

        _mapperMock.Setup(x => x.Map<CartDto>(It.IsAny<(string, CartModel)>()))
            .Returns(((string CartKey, CartModel Cart) input) => new CartDto
            {
                CartKey = input.CartKey,
                TotalItems = input.Cart.Items.Sum(i => i.Quantity)
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].ProductId.Should().Be(2);
        cart.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCartNotFound()
    {
        _cartRepoMock.Setup(x => x.GetCartAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartModel?)null);

        var command = new RemoveFromCartCommand
        {
            CartKey = "nonexistent",
            ProductId = 1
        };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cart not found for key: nonexistent");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenItemNotFound()
    {
        var cart = new CartModel
        {
            Id = "cart:user:1",
            Items = new List<CartItemModel>
            {
                new() { ProductId = 1, Quantity = 1, UnitPrice = 10m, Name = "Item" }
            }
        };

        _cartRepoMock.Setup(x => x.GetCartAsync("cart:user:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var command = new RemoveFromCartCommand
        {
            CartKey = "cart:user:1",
            ProductId = 999
        };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Item not found in cart");
    }
}
