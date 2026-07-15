using MediatR;
using MassTransit;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.Common.Models;
using CommerceHub.Modules.Payment.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Payment.Application.Commands;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, RefundResult>
{
    private readonly IPaymentDbContext _context;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IPublishEndpoint _publishEndpoint;

    public RefundPaymentCommandHandler(
        IPaymentDbContext context,
        IPaymentGatewayFactory gatewayFactory,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _gatewayFactory = gatewayFactory;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<RefundResult> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments.FindAsync(new object[] { request.PaymentId }, cancellationToken);
        if (payment == null)
        {
            return new RefundResult
            {
                Success = false,
                ErrorMessage = "Payment not found",
                Status = "Failed"
            };
        }

        var gateway = _gatewayFactory.GetGateway(payment.PaymentMethod);
        RefundResult refundResult;

        try
        {
            var paymentResult = await gateway.RefundPaymentAsync(payment.PaymentIntentId ?? payment.TransactionId ?? "", request.Amount);
            refundResult = new RefundResult
            {
                Success = paymentResult.Success,
                RefundId = paymentResult.TransactionId,
                AmountRefunded = request.Amount,
                ErrorMessage = paymentResult.ErrorMessage,
                Status = paymentResult.Success ? "Refunded" : "Failed"
            };
        }
        catch (Exception ex)
        {
            refundResult = new RefundResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "Failed"
            };
        }

        if (refundResult.Success)
        {
            var refund = new Refund
            {
                PaymentId = request.PaymentId,
                Amount = request.Amount,
                Reason = request.Reason,
                RefundTransactionId = refundResult.RefundId,
                Status = refundResult.Status,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Refunds.Add(refund);
            payment.Status = "Refunded";
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new PaymentRefunded
            {
                PaymentId = request.PaymentId,
                OrderId = payment.OrderId,
                RefundAmount = request.Amount,
                RefundedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        return refundResult;
    }
}
