using CommerceHub.Modules.Payment.Application.Commands;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.Common.Models;
using CommerceHub.Shared.Contracts.Events;
using CommerceHub.Modules.Payment.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using CommerceHub.TestBase;
using FluentAssertions;
using Moq;
using Xunit;
using PaymentEntity = CommerceHub.Modules.Payment.Domain.Entities.Payment;

namespace CommerceHub.Modules.Payment.UnitTests;

public class RefundPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentDbContext> _contextMock;
    private readonly Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly RefundPaymentCommandHandler _handler;

    public RefundPaymentCommandHandlerTests()
    {
        _contextMock = new Mock<IPaymentDbContext>();
        _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _gatewayMock = new Mock<IPaymentGateway>();

        _gatewayFactoryMock.Setup(x => x.GetGateway(It.IsAny<string>())).Returns(_gatewayMock.Object);

        _handler = new RefundPaymentCommandHandler(
            _contextMock.Object,
            _gatewayFactoryMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRefundPayment_WhenPaymentExists()
    {
        var payment = new PaymentEntity
        {
            Id = 1,
            OrderId = 1,
            Amount = 100.00m,
            PaymentMethod = "Stripe",
            Status = "Completed"
        };
        var payments = new List<PaymentEntity> { payment }.AsQueryable();

        var mockPayments = new Mock<DbSet<PaymentEntity>>();
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.Provider).Returns(payments.Provider);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.Expression).Returns(payments.Expression);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.ElementType).Returns(payments.ElementType);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.GetEnumerator()).Returns(payments.GetEnumerator());

        mockPayments.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _contextMock.Setup(x => x.Payments).Returns(mockPayments.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var refunds = new List<Refund>().AsQueryable();
        var mockRefunds = new Mock<DbSet<Refund>>();
        mockRefunds.As<IQueryable<Refund>>().Setup(m => m.Provider).Returns(refunds.Provider);
        mockRefunds.As<IQueryable<Refund>>().Setup(m => m.Expression).Returns(refunds.Expression);
        mockRefunds.As<IQueryable<Refund>>().Setup(m => m.ElementType).Returns(refunds.ElementType);
        mockRefunds.As<IQueryable<Refund>>().Setup(m => m.GetEnumerator()).Returns(refunds.GetEnumerator());
        _contextMock.Setup(x => x.Refunds).Returns(mockRefunds.Object);

        _gatewayMock.Setup(x => x.RefundPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new PaymentResult
            {
                Success = true,
                TransactionId = "refund_123",
                Status = "Completed"
            });

        var command = new RefundPaymentCommand
        {
            PaymentId = 1,
            Amount = 100m,
            Reason = "Customer request"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.RefundId.Should().Be("refund_123");
        payment.Status.Should().Be("Refunded");
        _contextMock.Verify(x => x.Refunds.Add(It.IsAny<Refund>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<PaymentRefunded>(r => r.RefundAmount == 100m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPaymentNotFound()
    {
        var payments = new List<PaymentEntity>().AsQueryable();
        var mockPayments = new Mock<DbSet<PaymentEntity>>();
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.Provider).Returns(payments.Provider);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.Expression).Returns(payments.Expression);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.ElementType).Returns(payments.ElementType);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.GetEnumerator()).Returns(payments.GetEnumerator());

        mockPayments.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEntity?)null);

        _contextMock.Setup(x => x.Payments).Returns(mockPayments.Object);

        var command = new RefundPaymentCommand { PaymentId = 999, Amount = 50m };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Payment not found");
    }

    [Fact]
    public async Task Handle_ShouldHandleGatewayException()
    {
        var payment = new PaymentEntity { Id = 1, OrderId = 1, Amount = 100m, PaymentMethod = "Stripe" };
        var payments = new List<PaymentEntity> { payment }.AsQueryable();
        var mockPayments = new Mock<DbSet<PaymentEntity>>();
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.Provider).Returns(payments.Provider);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.Expression).Returns(payments.Expression);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.ElementType).Returns(payments.ElementType);
        mockPayments.As<IQueryable<PaymentEntity>>().Setup(m => m.GetEnumerator()).Returns(payments.GetEnumerator());
        mockPayments.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _contextMock.Setup(x => x.Payments).Returns(mockPayments.Object);

        _gatewayMock.Setup(x => x.RefundPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()))
            .ThrowsAsync(new Exception("Gateway error"));

        var command = new RefundPaymentCommand { PaymentId = 1, Amount = 100m };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Gateway error");
    }
}
