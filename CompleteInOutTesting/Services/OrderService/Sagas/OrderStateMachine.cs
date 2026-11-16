using BuildingBlocks.Messaging.Contracts.Orders;
using MassTransit;

namespace OrderService.Sagas;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State WaitingForInventory { get; private set; } = null!;
    public State WaitingForPayment { get; private set; } = null!;
    public State WaitingForShipment { get; private set; } = null!;
    public State CompensatingPayment { get; private set; } = null!;
    public State CompensatingInventory { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<SubmitOrder> OrderSubmitted { get; private set; } = null!;
    public Event<InventoryReserved> InventoryWasReserved { get; private set; } = null!;
    public Event<InventoryReservationFailed> InventoryReservationFailed { get; private set; } = null!;
    public Event<PaymentAuthorized> PaymentWasAuthorized { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFailed { get; private set; } = null!;
    public Event<ShipmentCreated> ShipmentWasCreated { get; private set; } = null!;
    public Event<ShipmentFailed> ShipmentFailed { get; private set; } = null!;
    public Event<PaymentVoided> PaymentWasVoided { get; private set; } = null!;
    public Event<InventoryReleased> InventoryWasReleased { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted, e =>
        {
            e.CorrelateById(context => context.Message.OrderId);
            e.InsertOnInitial = true;
        });

        Event(() => InventoryWasReserved, e => e.CorrelateById(context => context.Message.OrderId));
        Event(() => InventoryReservationFailed, e => e.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentWasAuthorized, e => e.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentFailed, e => e.CorrelateById(context => context.Message.OrderId));
        Event(() => ShipmentWasCreated, e => e.CorrelateById(context => context.Message.OrderId));
        Event(() => ShipmentFailed, e => e.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentWasVoided, e => e.CorrelateById(context => context.Message.OrderId));
        Event(() => InventoryWasReleased, e => e.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.OrderId;
                    context.Saga.ProductId = context.Message.ProductId;
                    context.Saga.Quantity = context.Message.Quantity;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.SimulateShippingFailure = context.Message.SimulateShippingFailure;
                })
                .Publish(context => new ReserveInventory(
                    context.Saga.CorrelationId,
                    context.Saga.ProductId,
                    context.Saga.Quantity))
                .TransitionTo(WaitingForInventory));

        During(WaitingForInventory,
            When(InventoryWasReserved)
                .Publish(context => new AuthorizePayment(
                    context.Saga.CorrelationId,
                    context.Saga.Amount))
                .TransitionTo(WaitingForPayment),

            When(InventoryReservationFailed)
                .Then(context => context.Saga.FailureReason = context.Message.Reason)
                .TransitionTo(Failed));

        During(WaitingForPayment,
            When(PaymentWasAuthorized)
                .Then(context => context.Saga.TransactionId = context.Message.TransactionId)
                .Publish(context => new CreateShipment(
                    context.Saga.CorrelationId,
                    context.Saga.SimulateShippingFailure))
                .TransitionTo(WaitingForShipment),

            When(PaymentFailed)
                .Then(context => context.Saga.FailureReason = context.Message.Reason)
                .Publish(context => new ReleaseInventory(
                    context.Saga.CorrelationId,
                    context.Saga.ProductId,
                    context.Saga.Quantity))
                .TransitionTo(CompensatingInventory));

        During(WaitingForShipment,
            When(ShipmentWasCreated)
                .Then(context => context.Saga.ShipmentId = context.Message.ShipmentId)
                .TransitionTo(Completed),

            When(ShipmentFailed)
                .Then(context => context.Saga.FailureReason = context.Message.Reason)
                .Publish(context => new VoidPayment(
                    context.Saga.CorrelationId,
                    context.Saga.TransactionId!))
                .TransitionTo(CompensatingPayment));

        During(CompensatingPayment,
            When(PaymentWasVoided)
                .Publish(context => new ReleaseInventory(
                    context.Saga.CorrelationId,
                    context.Saga.ProductId,
                    context.Saga.Quantity))
                .TransitionTo(CompensatingInventory));

        During(CompensatingInventory,
            When(InventoryWasReleased)
                .TransitionTo(Failed));
    }
}
