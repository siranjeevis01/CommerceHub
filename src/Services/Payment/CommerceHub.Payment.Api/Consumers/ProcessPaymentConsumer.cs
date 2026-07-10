using CommerceHub.Payment.Application.Commands;
using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using MediatR;

namespace CommerceHub.Payment.Api.Consumers;

public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessPaymentConsumer> _logger;

    public ProcessPaymentConsumer(IMediator mediator, ILogger<ProcessPaymentConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        _logger.LogInformation("Processing payment for Order {OrderId}, Amount {Amount}",
            context.Message.OrderId, context.Message.Amount);

        var command = new ProcessPaymentCommand
        {
            OrderId = context.Message.OrderId,
            UserId = context.Message.UserId,
            Amount = context.Message.Amount,
            Currency = context.Message.Currency,
            Provider = context.Message.Provider
        };

        await _mediator.Send(command, context.CancellationToken);
    }
}
