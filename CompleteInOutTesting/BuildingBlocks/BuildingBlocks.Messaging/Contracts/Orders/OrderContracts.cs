namespace BuildingBlocks.Messaging.Contracts.Orders;

public record SubmitOrder(Guid OrderId, int ProductId, int Quantity, decimal Amount, bool SimulateShippingFailure = false);

public record ReserveInventory(Guid OrderId, int ProductId, int Quantity);
public record InventoryReserved(Guid OrderId);
public record InventoryReservationFailed(Guid OrderId, string Reason);
public record ReleaseInventory(Guid OrderId, int ProductId, int Quantity);
public record InventoryReleased(Guid OrderId);

public record AuthorizePayment(Guid OrderId, decimal Amount);
public record PaymentAuthorized(Guid OrderId, string TransactionId);
public record PaymentFailed(Guid OrderId, string Reason);
public record VoidPayment(Guid OrderId, string TransactionId);
public record PaymentVoided(Guid OrderId);

public record CreateShipment(Guid OrderId, bool SimulateFailure);
public record ShipmentCreated(Guid OrderId, string ShipmentId);
public record ShipmentFailed(Guid OrderId, string Reason);
