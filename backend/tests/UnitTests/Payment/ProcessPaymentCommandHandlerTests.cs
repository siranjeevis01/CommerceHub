using AutoMapper;
using CommerceHub.Modules.Payment.Application.Commands;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.Common.Models;
using CommerceHub.Modules.Payment.Application.DTOs;
using CommerceHub.Shared.Contracts.Events;
using CommerceHub.TestBase;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using PaymentEntity = CommerceHub.Modules.Payment.Domain.Entities.Payment;

namespace CommerceHub.Modules.Payment.UnitTests;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentDbContext> _contextMock;
    private readonly Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _contextMock = new Mock<IPaymentDbContext>();
        _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
        _mapperMock = new Mock<IMapper>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _gatewayMock = new Mock<IPaymentGateway>();

        _gatewayFactoryMock.Setup(x => x.GetGateway(It.IsAny<string>())).Returns(_gatewayMock.Object);

        _handler = new ProcessPaymentCommandHandler(
            _contextMock.Object,
            _gatewayFactoryMock.Object,
            _mapperMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldProcessPayment_WhenGatewaySucceeds()
    {
        var payments = new List<PaymentEntity>().AsQueryable();
        var mockPayments = payments.BuildMockDbSet<PaymentEntity>();
        _contextMock.Setup(x => x.Payments).Returns(mockPayments.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _gatewayMock.Setup(x => x.CreatePaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new PaymentResult
            {
                Success = true,
                TransactionId = "txn_123",
                ClientSecret = null,
                Status = "Completed"
            });

        var command = new ProcessPaymentCommand
        {
            OrderId = 1,
            UserId = 1,
            Amount = 150.00m,
            Provider = "Stripe",
            PaymentMethodId = "pm_123",
            Currency = "usd"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("Completed");
        result.TransactionId.Should().Be("txn_123");
        result.RequiresAction.Should().BeFalse();
        _contextMock.Verify(x => x.Payments.Add(It.IsAny<PaymentEntity>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<PaymentConfirmed>(p => p.TransactionId == "txn_123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleGatewayFailure()
    {
        var payments = new List<PaymentEntity>().AsQueryable();
        var mockPayments = payments.BuildMockDbSet<PaymentEntity>();
        _contextMock.Setup(x => x.Payments).Returns(mockPayments.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _gatewayMock.Setup(x => x.CreatePaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Card declined",
                Status = "Failed"
            });

        var command = new ProcessPaymentCommand
        {
            OrderId = 1,
            UserId = 1,
            Amount = 100.00m,
            Provider = "Stripe",
            Currency = "usd"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be("Failed");
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<PaymentFailed>(p => p.FailureReason == "Card declined"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleGatewayException()
    {
        var payments = new List<PaymentEntity>().AsQueryable();
        var mockPayments = payments.BuildMockDbSet<PaymentEntity>();
        _contextMock.Setup(x => x.Payments).Returns(mockPayments.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _gatewayMock.Setup(x => x.CreatePaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new Exception("Gateway timeout"));

        var command = new ProcessPaymentCommand
        {
            OrderId = 1,
            UserId = 1,
            Amount = 100.00m,
            Provider = "Stripe"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be("Failed");
    }

    [Fact]
    public async Task Handle_ShouldRequireAction_WhenClientSecretPresent()
    {
        var payments = new List<PaymentEntity>().AsQueryable();
        var mockPayments = payments.BuildMockDbSet<PaymentEntity>();
        _contextMock.Setup(x => x.Payments).Returns(mockPayments.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _gatewayMock.Setup(x => x.CreatePaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new PaymentResult
            {
                Success = true,
                TransactionId = "txn_req",
                ClientSecret = "secret_123",
                Status = "RequiresAction"
            });

        var command = new ProcessPaymentCommand
        {
            OrderId = 1,
            UserId = 1,
            Amount = 200.00m,
            Provider = "Stripe",
            Currency = "usd"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.RequiresAction.Should().BeTrue();
        result.ClientSecret.Should().Be("secret_123");
    }
}
