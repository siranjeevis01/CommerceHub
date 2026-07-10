using MediatR;
using AutoMapper;
using MassTransit;
using CommerceHub.Payment.Application.Common.Interfaces;
using CommerceHub.Payment.Application.Common.Models;
using CommerceHub.Payment.Application.DTOs;
using CommerceHub.Payment.Domain.Entities;
using CommerceHub.Shared.Messaging.Events;

namespace CommerceHub.Payment.Application.Commands;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentIntentDto>
{
    private readonly IPaymentDbContext _context;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public ProcessPaymentCommandHandler(
        IPaymentDbContext context,
        IPaymentGatewayFactory gatewayFactory,
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _gatewayFactory = gatewayFactory;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<PaymentIntentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var gateway = _gatewayFactory.GetGateway(request.Provider);
        PaymentResult paymentResult;

        try
        {
            var metadata = new Dictionary<string, string>
            {
                { "OrderId", request.OrderId.ToString() },
                { "UserId", request.UserId.ToString() },
                { "PaymentMethodId", request.PaymentMethodId },
                { "ReturnUrl", request.ReturnUrl ?? "" }
            };
            paymentResult = await gateway.CreatePaymentAsync(request.Amount, request.Currency, metadata);
        }
        catch (Exception ex)
        {
            paymentResult = new PaymentResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "Failed"
            };
        }

        var payment = new Domain.Entities.Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            PaymentMethod = request.Provider,
            TransactionId = paymentResult.TransactionId,
            Status = paymentResult.Success ? (paymentResult.ClientSecret != null ? "RequiresAction" : "Completed") : "Failed",
            FailureReason = paymentResult.ErrorMessage,
            PaymentIntentId = null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        if (paymentResult.Success)
        {
            await _publishEndpoint.Publish(new PaymentConfirmed
            {
                PaymentId = payment.Id,
                OrderId = request.OrderId,
                UserId = request.UserId,
                TransactionId = paymentResult.TransactionId,
                PaymentMethod = request.Provider,
                Amount = request.Amount,
                ConfirmedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            await _publishEndpoint.Publish(new PaymentFailed
            {
                PaymentId = payment.Id,
                OrderId = request.OrderId,
                UserId = request.UserId,
                FailureReason = paymentResult.ErrorMessage ?? "Unknown error",
                FailedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        return new PaymentIntentDto
        {
            PaymentId = payment.Id,
            ClientSecret = paymentResult.ClientSecret,
            Status = payment.Status,
            TransactionId = paymentResult.TransactionId,
            RequiresAction = paymentResult.ClientSecret != null
        };
    }
}
