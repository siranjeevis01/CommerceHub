using MediatR;
using AutoMapper;
using MassTransit;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.DTOs;
using CommerceHub.Modules.Payment.Domain.Entities;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Payment.Application.Commands;

public class HandleWebhookCommandHandler : IRequestHandler<HandleWebhookCommand, PaymentIntentDto>
{
    private readonly IPaymentDbContext _context;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public HandleWebhookCommandHandler(
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

    public async Task<PaymentIntentDto> Handle(HandleWebhookCommand request, CancellationToken cancellationToken)
    {
        var gateway = _gatewayFactory.GetGateway(request.Provider);
        var result = await gateway.ConfirmWebhookAsync(request.Payload, request.Signature, cancellationToken);

        if (!result.Success)
        {
            var failedPayment = new Domain.Entities.Payment
            {
                Status = "Failed",
                FailureReason = result.ErrorMessage,
                Amount = 0,
                PaymentMethod = request.Provider,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Payments.Add(failedPayment);
            await _context.SaveChangesAsync(cancellationToken);

            return new PaymentIntentDto
            {
                PaymentId = failedPayment.Id,
                Status = "Failed",
                RequiresAction = false
            };
        }

        var payment = _context.Payments.FirstOrDefault(p => p.TransactionId == result.TransactionId);
        if (payment == null)
        {
            payment = new Domain.Entities.Payment
            {
                TransactionId = result.TransactionId,
                Status = result.Status,
                PaymentMethod = request.Provider,
                Amount = 0,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Payments.Add(payment);
        }
        else
        {
            payment.Status = result.Status;
            payment.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new PaymentConfirmed
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            TransactionId = result.TransactionId,
            PaymentMethod = request.Provider,
            Amount = payment.Amount,
            ConfirmedAt = DateTime.UtcNow
        }, cancellationToken);

        return new PaymentIntentDto
        {
            PaymentId = payment.Id,
            Status = payment.Status,
            TransactionId = result.TransactionId,
            RequiresAction = false
        };
    }
}
