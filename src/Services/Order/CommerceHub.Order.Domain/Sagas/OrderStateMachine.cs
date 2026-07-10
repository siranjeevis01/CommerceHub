using MassTransit;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Order.Domain.Sagas;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State? PendingInventory { get; set; }
    public State? PendingPayment { get; set; }
    public State? Confirmed { get; set; }
    public State? Shipped { get; set; }
    public State? Delivered { get; set; }
    public State? Cancelled { get; set; }
    public State? Failed { get; set; }

    public Event<OrderPlaced>? OrderPlaced { get; set; }
    public Event<InventoryReserved>? InventoryReserved { get; set; }
    public Event<InventoryFailed>? InventoryFailed { get; set; }
    public Event<PaymentConfirmed>? PaymentConfirmed { get; set; }
    public Event<PaymentFailed>? PaymentFailed { get; set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x =>
        {
            x.CorrelateById(context => context.Message.CorrelationId);
            x.CorrelateBy((saga, context) => saga.OrderId == context.Message.OrderId);
        });
        Event(() => InventoryReserved, x => x.CorrelateBy((saga, context) => saga.OrderId == context.Message.OrderId));
        Event(() => InventoryFailed, x => x.CorrelateBy((saga, context) => saga.OrderId == context.Message.OrderId));
        Event(() => PaymentConfirmed, x => x.CorrelateBy((saga, context) => saga.OrderId == context.Message.OrderId));
        Event(() => PaymentFailed, x => x.CorrelateBy((saga, context) => saga.OrderId == context.Message.OrderId));

        Initially(
            When(OrderPlaced)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.OrderNumber = context.Message.OrderNumber;
                    context.Saga.UserId = context.Message.UserId;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .TransitionTo(PendingInventory)
        );

        During(PendingInventory,
            When(InventoryReserved)
                .TransitionTo(PendingPayment)
                .PublishAsync(context => context.Init<ProcessPayment>(new
                {
                    OrderId = context.Saga.OrderId,
                    UserId = context.Saga.UserId,
                    Amount = context.Saga.TotalAmount,
                    Currency = "USD",
                    Provider = "Stripe"
                })),
            When(InventoryFailed)
                .Then(context => context.Saga.FailureReason = context.Message.Reason)
                .TransitionTo(Failed)
                .PublishAsync(context => context.Init<OrderCancelled>(new
                {
                    OrderId = context.Saga.OrderId,
                    OrderNumber = context.Saga.OrderNumber,
                    UserId = context.Saga.UserId,
                    Reason = context.Message.Reason,
                    CancelledAt = DateTime.UtcNow
                }))
        );

        During(PendingPayment,
            When(PaymentConfirmed)
                .Then(context => context.Saga.PaymentId = context.Message.PaymentId)
                .TransitionTo(Confirmed)
                .PublishAsync(context => context.Init<OrderConfirmed>(new
                {
                    OrderId = context.Saga.OrderId,
                    OrderNumber = context.Saga.OrderNumber,
                    UserId = context.Saga.UserId,
                    ConfirmedAt = DateTime.UtcNow
                })),
            When(PaymentFailed)
                .Then(context => context.Saga.FailureReason = context.Message.FailureReason)
                .TransitionTo(Failed)
                .PublishAsync(context => context.Init<OrderCancelled>(new
                {
                    OrderId = context.Saga.OrderId,
                    OrderNumber = context.Saga.OrderNumber,
                    UserId = context.Saga.UserId,
                    Reason = context.Message.FailureReason,
                    CancelledAt = DateTime.UtcNow
                }))
        );
    }
}
