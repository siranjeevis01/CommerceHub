using AutoMapper;
using CommerceHub.Cart.Application.Commands;
using CommerceHub.Cart.Application.Common.Interfaces;
using CommerceHub.Cart.Application.DTOs;
using FluentAssertions;
using MassTransit;
using Moq;
using Xunit;
using CartModel = CommerceHub.Cart.Domain.Models.Cart;
using CartItemModel = CommerceHub.Cart.Domain.Models.CartItem;

namespace CommerceHub.Cart.UnitTests;

public class ApplyCouponCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ApplyCouponCommandHandler _handler;

    public ApplyCouponCommandHandlerTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _mapperMock = new Mock<IMapper>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new ApplyCouponCommandHandler(_cartRepoMock.Object, _mapperMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldApplyCoupon_WhenCartExists()
    {
        var cart = new CartModel
        {
            Id = "cart:user:1",
            Items = new List<CartItemModel>
            {
                new() { ProductId = 1, UnitPrice = 100.00m, Quantity = 1, Name = "Expensive Item" }
            }
        };

        _cartRepoMock.Setup(x => x.GetCartAsync("cart:user:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepoMock.Setup(x => x.SaveCartAsync(It.IsAny<string>(), It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ApplyCouponCommand
        {
            CartKey = "cart:user:1",
            CouponCode = "SAVE10"
        };

        _mapperMock.Setup(x => x.Map<CartDto>(It.IsAny<(string, CartModel)>()))
            .Returns(((string CartKey, CartModel Cart) input) => new CartDto
            {
                CartKey = input.CartKey,
                CouponCode = input.Cart.CouponCode,
                DiscountAmount = input.Cart.DiscountAmount,
                SubTotal = input.Cart.Items.Sum(i => i.TotalPrice)
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        cart.CouponCode.Should().Be("SAVE10");
        cart.DiscountAmount.Should().Be(10.00m);
        cart.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldUppercaseCouponCode()
    {
        var cart = new CartModel
        {
            Id = "cart:user:2",
            Items = new List<CartItemModel>
            {
                new() { ProductId = 1, UnitPrice = 50.00m, Quantity = 1, Name = "Item" }
            }
        };

        _cartRepoMock.Setup(x => x.GetCartAsync("cart:user:2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepoMock.Setup(x => x.SaveCartAsync(It.IsAny<string>(), It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(x => x.Map<CartDto>(It.IsAny<(string, CartModel)>()))
            .Returns(((string CartKey, CartModel Cart) input) => new CartDto { CouponCode = input.Cart.CouponCode });

        var command = new ApplyCouponCommand
        {
            CartKey = "cart:user:2",
            CouponCode = "save20"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        cart.CouponCode.Should().Be("SAVE20");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCartNotFound()
    {
        _cartRepoMock.Setup(x => x.GetCartAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartModel?)null);

        var command = new ApplyCouponCommand
        {
            CartKey = "missing",
            CouponCode = "CODE"
        };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cart not found for key: missing");
    }

    [Fact]
    public async Task Handle_ShouldCalculateDiscountBasedOnSubtotal()
    {
        var cart = new CartModel
        {
            Id = "cart:user:3",
            Items = new List<CartItemModel>
            {
                new() { ProductId = 1, UnitPrice = 200.00m, Quantity = 2, Name = "Item A" },
                new() { ProductId = 2, UnitPrice = 50.00m, Quantity = 3, Name = "Item B" }
            }
        };

        _cartRepoMock.Setup(x => x.GetCartAsync("cart:user:3", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepoMock.Setup(x => x.SaveCartAsync(It.IsAny<string>(), It.IsAny<CartModel>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(x => x.Map<CartDto>(It.IsAny<(string, CartModel)>()))
            .Returns(((string CartKey, CartModel Cart) input) => new CartDto { DiscountAmount = input.Cart.DiscountAmount });

        var command = new ApplyCouponCommand
        {
            CartKey = "cart:user:3",
            CouponCode = "BIG20"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        cart.DiscountAmount.Should().Be(55.00m);
    }
}
