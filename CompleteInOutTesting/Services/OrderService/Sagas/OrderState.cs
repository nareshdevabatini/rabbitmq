using MassTransit;

namespace OrderService.Sagas;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public bool SimulateShippingFailure { get; set; }

    public string? TransactionId { get; set; }
    public string? ShipmentId { get; set; }
    public string? FailureReason { get; set; }
}

