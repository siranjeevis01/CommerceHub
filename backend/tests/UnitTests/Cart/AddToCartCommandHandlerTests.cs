using AutoMapper;
using CommerceHub.Modules.Cart.Application.Commands;
using CommerceHub.Modules.Cart.Application.Common.Interfaces;
using CommerceHub.Modules.Cart.Application.DTOs;
using FluentAssertions;
using Moq;
using Xunit;
using CartModel = CommerceHub.Modules.Cart.Domain.Models.Cart;
using CartItemModel = CommerceHub.Modules.Cart.Domain.Models.CartItem;

namespace CommerceHub.Modules.Cart.UnitTests;

public class AddToCartCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AddToCartCommandHandler _handler;

    public AddToCartCommandHandlerTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new AddToCartCommandHandler(_cartRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddNewItem_WhenCartExistsAndItemNotInCart()
    {
        var cart = new CartModel
        {
            Id = "cart:user:1",
            UserId = 1,
            Items = new List<CartItemModel>()
        };

        _cartRepoMock.Setup(x => x.GetCartAsync("cart:user:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepoMock.Setup(x => x.SaveCartAsync(It.IsAny<string>(), It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AddToCartCommand
        {
            CartKey = "cart:user:1",
            ProductId = 1,
            ProductName = "Test Product",
            UnitPrice = 25.00m,
            Quantity = 2
        };

        _mapperMock.Setup(x => x.Map<CartDto>(It.IsAny<(string, CartModel)>()))
            .Returns(((string CartKey, CartModel Cart) input) => new CartDto
            {
                CartKey = input.CartKey,
                TotalItems = input.Cart.Items.Sum(i => i.Quantity),
                SubTotal = input.Cart.Items.Sum(i => i.TotalPrice)
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        cart.Items.Should().HaveCount(1);
        cart.Items[0].ProductId.Should().Be(1);
        cart.Items[0].Quantity.Should().Be(2);
        cart.Items[0].TotalPrice.Should().Be(50.00m);
        cart.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCreateNewCart_WhenCartDoesNotExist()
    {
        _cartRepoMock.Setup(x => x.GetCartAsync("cart:session:abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartModel?)null);

        _cartRepoMock.Setup(x => x.SaveCartAsync(It.IsAny<string>(), It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AddToCartCommand
        {
            CartKey = "cart:session:abc",
            ProductId = 5,
            ProductName = "New Product",
            UnitPrice = 10.00m,
            Quantity = 1
        };

        CartModel? savedCart = null;
        _cartRepoMock.Setup(x => x.SaveCartAsync("cart:session:abc", It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Callback<string, CartModel, TimeSpan?, CancellationToken>((_, c, _, _) => savedCart = c)
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(x => x.Map<CartDto>(It.IsAny<(string, CartModel)>()))
            .Returns(((string CartKey, CartModel Cart) input) => new CartDto
            {
                CartKey = input.CartKey,
                TotalItems = input.Cart.Items.Sum(i => i.Quantity),
                SubTotal = input.Cart.Items.Sum(i => i.TotalPrice)
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        savedCart.Should().NotBeNull();
        savedCart!.Items.Should().HaveCount(1);
        savedCart.Items[0].ProductId.Should().Be(5);
        savedCart.Items[0].Quantity.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldIncrementQuantity_WhenItemAlreadyInCart()
    {
        var cart = new CartModel
        {
            Id = "cart:user:1",
            Items = new List<CartItemModel>
            {
                new() { ProductId = 1, VariantId = null, Name = "Product", UnitPrice = 25.00m, Quantity = 1 }
            }
        };

        _cartRepoMock.Setup(x => x.GetCartAsync("cart:user:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepoMock.Setup(x => x.SaveCartAsync(It.IsAny<string>(), It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AddToCartCommand
        {
            CartKey = "cart:user:1",
            ProductId = 1,
            Quantity = 3
        };

        _mapperMock.Setup(x => x.Map<CartDto>(It.IsAny<(string, CartModel)>()))
            .Returns(((string CartKey, CartModel Cart) input) => new CartDto
            {
                CartKey = input.CartKey,
                TotalItems = input.Cart.Items.Sum(i => i.Quantity)
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(4);
    }

    [Fact]
    public async Task Handle_ShouldHandleMultipleItems()
    {
        var cart = new CartModel
        {
            Id = "cart:user:1",
            Items = new List<CartItemModel>()
        };

        _cartRepoMock.Setup(x => x.GetCartAsync("cart:user:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepoMock.Setup(x => x.SaveCartAsync(It.IsAny<string>(), It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command1 = new AddToCartCommand
        {
            CartKey = "cart:user:1",
            ProductId = 1,
            ProductName = "Item 1",
            UnitPrice = 10.00m,
            Quantity = 2
        };

        var command2 = new AddToCartCommand
        {
            CartKey = "cart:user:1",
            ProductId = 2,
            ProductName = "Item 2",
            UnitPrice = 20.00m,
            Quantity = 1
        };

        _mapperMock.Setup(x => x.Map<CartDto>(It.IsAny<(string, CartModel)>()))
            .Returns(((string CartKey, CartModel Cart) input) => new CartDto
            {
                CartKey = input.CartKey,
                TotalItems = input.Cart.Items.Sum(i => i.Quantity),
                SubTotal = input.Cart.Items.Sum(i => i.TotalPrice)
            });

        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        cart.Items.Should().HaveCount(2);
        cart.Items.Sum(i => i.Quantity).Should().Be(3);
        cart.Items.Sum(i => i.TotalPrice).Should().Be(40.00m);
    }
}
