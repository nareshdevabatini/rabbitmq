# CompleteInOutTesting — RabbitMQ, Saga and Outbox Learning Project

This is a learning roadmap tailored to the existing `.NET 10` solution, which currently uses MassTransit `8.3.6`, RabbitMQ, EF Core with SQLite, Redis, Carter and MediatR.

## 1. Existing project understanding

The current solution contains:

- `Services/EmployeeService` — Carter endpoints, MediatR, EF Core SQLite and Redis caching.
- `Services/ProductService` — Carter endpoints, MediatR and EF Core SQLite.
- `BuildingBlocks/BuildingBlocks.Messaging` — shared MassTransit registration, RabbitMQ host configuration and event contracts.
- `BuildingBlocks/BuildingBlocks` — shared CQRS contracts and exception-handling components.
- Docker Compose services for EmployeeService, ProductService, Redis and RabbitMQ Management.

### Existing message flow

1. `ProductService` handles `ProductCommand`.
2. `CreateProductCommandHandler` saves the Product through `IProductRepository.AddProduct`.
3. The handler separately publishes `AddProductEvent` using `IPublishEndpoint`.
4. `EmployeeService` consumes `AddProductEvent` in `AddEmployeeProductHandler`.

The shared `AddMessageBroker` extension registers consumers from each service assembly and calls `ConfigureEndpoints(context)`. RabbitMQ connection values are read from `MessageBroker:Host`, `MessageBroker:UserName` and `MessageBroker:Password`.

**Current reliability gap:** Product persistence and event publishing are separate operations. If the database commit succeeds and RabbitMQ publishing fails, the Product exists but the event is not delivered. Part 6 is intended to close this dual-write gap with a Transactional Outbox.

Keep the current Employee/Product example as the simple producer/consumer demonstration. Add a separate Order workflow for Saga orchestration so that business coordination and compensation are explicit.

## 2. Learning roadmap

| Module | Concepts | Project deliverable |
|---|---|---|
| Part 1 | AMQP fundamentals, exchanges, queues, bindings | Understand and inspect RabbitMQ topology |
| Part 2 | Direct, Fanout, Topic and Headers routing | Publish test messages using each routing strategy |
| Part 3 | Dead Letter Exchange (DLX) and Dead Letter Queue (DLQ) | Capture rejected, expired or max-length messages |
| Part 4 | Retry, exponential backoff and delayed redelivery | Handle transient failures with bounded retries |
| Part 5 | Saga orchestration and compensation | Coordinate Order, Inventory, Payment and Shipping |
| Part 6 | Transactional Outbox Pattern | Persist business data and outgoing messages atomically |
| Part 7 | Consumer Outbox and idempotency | Prevent duplicate business side effects on redelivery |
| Part 8 | Failure handling and distributed transaction consistency | Test outages, duplicates, restarts and compensation failures |

---

# Part 1 — AMQP fundamentals, exchanges, queues and bindings

## Concepts

AMQP is the messaging protocol used by RabbitMQ clients. In the AMQP 0-9-1 model, a producer publishes to an exchange. The exchange routes messages to zero or more queues according to the exchange type and bindings.

- **Producer:** publishes a message.
- **Exchange:** routes published messages.
- **Queue:** stores messages until consumed or expired.
- **Binding:** connects an exchange to a queue, optionally with a routing key and arguments.
- **Routing key:** message metadata used by exchange routing.
- **Acknowledgement (ack):** confirms successful processing.
- **Negative acknowledgement (nack/reject):** indicates unsuccessful processing; the flags and queue configuration determine requeue/dead-letter behavior.
- **Virtual host (vhost):** namespace that isolates users, exchanges, queues and bindings.

## Practical exercises

1. Open RabbitMQ Management at `http://localhost:15672`.
2. Inspect Exchanges, Queues, Bindings, Connections and Channels.
3. Create a durable direct exchange, durable queue and binding.
4. Publish a message with a matching routing key.
5. Publish with a non-matching routing key and observe that the bound queue does not receive it.

**Expected result:** explain the producer → exchange → binding → queue → consumer flow and distinguish AMQP port `5672` from Management UI port `15672`.

# Part 2 — Direct, Fanout, Topic and Headers routing

| Exchange type | Routing rule | Example |
|---|---|---|
| Direct | Exact routing-key match | `employee.created` |
| Fanout | Copies to every bound queue; routing key is ignored | Broadcast a notification |
| Topic | Routing-key pattern matching using `*` and `#` | `employee.*`, `employee.#` |
| Headers | Matches message headers and binding arguments | `event-type=employee-created` |

## Practical exercises

- **Direct:** route `employee.created` to `employee-created-queue`.
- **Fanout:** publish one event and verify every bound queue receives a copy.
- **Topic:** publish `sales.audit.completed`; bind queues with `sales.#` and `sales.audit.*` and compare delivery.
- **Headers:** publish `event-type=employee-created` and test bindings using `x-match=all` and `x-match=any`.

**Expected result:** select the right exchange type for a routing requirement and troubleshoot why a queue did or did not receive a message.

# Part 3 — Dead Letter Exchange and Dead Letter Queue

A queue can dead-letter a message when it is rejected without requeue, expires because of TTL, or is dropped because of a queue length limit. The queue's dead-letter configuration names the dead-letter exchange and can specify a dead-letter routing key.

A DLX is an **exchange**, not a queue. A DLQ is an ordinary queue bound to that exchange.

```text
Producer
   |
   v
work-exchange
   |
   v
work-queue
   | reject/nack(requeue:false), expiry or max length
   v
 dead-letter-exchange
   |
   v
 dead-letter-queue
```

## Practical exercises

1. Create a work exchange and work queue.
2. Configure the work queue with `x-dead-letter-exchange` (or an equivalent broker policy).
3. Create a DLX and bind a DLQ to it.
4. Publish a message and deliberately reject it with requeue disabled.
5. Verify the DLQ message and inspect its `x-death` metadata.

**Expected result:** distinguish retry from dead-lettering. Avoid routing a poison message back to the same queue forever.

# Part 4 — Retry, exponential backoff and delayed redelivery

- **Immediate retry:** re-executes consumer processing in the current delivery context. Keep attempt counts small because the delivery/consumer slot can remain occupied.
- **Exponential backoff:** increases delay between attempts, for example 1s, 2s, 4s, 8s, with a maximum delay and optional jitter.
- **Delayed redelivery:** releases the current delivery and schedules a later redelivery. Use it for longer delays.
- **Poison message:** a message that repeatedly fails and should eventually move to an error/DLQ path for investigation.

## Practical exercises

1. Add a consumer that deliberately fails for a configurable number of attempts.
2. Configure a bounded immediate retry policy for transient exceptions.
3. Configure delayed redelivery with increasing intervals.
4. Make the consumer fail permanently and verify the configured error/DLQ path.
5. Log `MessageId`, `CorrelationId`, retry/redelivery count and exception details.

**Important:** MassTransit retry/redelivery middleware and RabbitMQ native dead-letter/TTL policies are different mechanisms. Avoid accidentally combining multiple retry layers that multiply the total attempts.

# Part 5 — Saga orchestration and compensation

## Business scenario

Add a separate Order workflow rather than changing the current Product notification example into a Saga.

```text
Submit Order
    |
    v
OrderSubmitted
    |
    v
Reserve Inventory
    |
    v
InventoryReserved
    |
    v
Authorize Payment
    |
    v
PaymentAuthorized
    |
    v
Create Shipment
    |
    v
ShipmentCreated
    |
    v
Order Completed
```

If payment fails after inventory reservation:

```text
PaymentFailed
    |
    v
Release Inventory (compensation)
    |
    v
Mark Order Failed
```

If shipping fails after payment authorization, the Saga should request appropriate compensating actions, such as payment void/refund and inventory release. Compensation is a business operation, not a database rollback spanning multiple services.

## Suggested message contracts

- `SubmitOrder`
- `OrderSubmitted`
- `ReserveInventory`
- `InventoryReserved`
- `InventoryReservationFailed`
- `AuthorizePayment`
- `PaymentAuthorized`
- `PaymentFailed`
- `CreateShipment`
- `ShipmentCreated`
- `ReleaseInventory`
- `VoidPayment`
- `OrderCompleted`
- `OrderFailed`

Include a stable `CorrelationId` (or `SagaId`) so all messages in one workflow can be correlated.

## Implementation guidance

Use a MassTransit state-machine Saga with persistent state. Keep Saga state in the Order service's database, not the Employee or Product database. Select a persistence provider supported by the installed MassTransit package set and verify the exact API against MassTransit 8.3.6.

# Part 6 — Transactional Outbox Pattern

## Problem in the current code

The current `CreateProductCommandHandler` saves through `IProductRepository.AddProduct` and then separately calls `IPublishEndpoint.Publish(new AddProductEvent(...))`. A failure between these operations can leave a persisted Product without its event.

## Target design

```text
One Product DB transaction
   |
   +-- Insert Product
   |
   +-- Insert Outbox message
   |
   +-- Commit
          |
          v
Outbox dispatcher
          |
          v
RabbitMQ
          |
          v
EmployeeService consumer
```

## Implementation requirements

1. Add Outbox persistence to the Product database (or use MassTransit Bus Outbox).
2. Save the Product and outgoing event in the same database transaction.
3. Run a dispatcher that publishes pending messages.
4. Mark an Outbox message delivered only after successful publication.
5. Handle dispatcher crashes safely; a message may be published more than once.
6. Define safe claiming/locking, retries and cleanup for multiple dispatcher instances.

Prefer MassTransit's Bus Outbox integration for MassTransit publishing. Verify registration and persistence APIs against version 8.3.6; do not copy APIs from newer major versions blindly.

# Part 7 — Consumer Outbox and idempotency

RabbitMQ delivery is generally at-least-once. A consumer may commit its database update and then fail before the acknowledgement reaches RabbitMQ. The message can be delivered again. The consumer must not repeat a non-idempotent side effect.

## Recommended design

- Give each incoming message a stable `MessageId`.
- Persist processed message identity in an Inbox/deduplication table with a unique constraint.
- Perform the business update and Inbox insert in the same local database transaction.
- Ignore duplicate message identities after confirming the original transaction committed.
- Use Consumer Outbox where appropriate to persist consumer state changes and outgoing messages atomically.

## Practical exercises

1. Publish a message with a known `MessageId`.
2. Process it once and persist the business effect.
3. Republish the same message with the same `MessageId`.
4. Verify that the second delivery does not duplicate the side effect.
5. Test simultaneous duplicate deliveries; the database uniqueness constraint must protect correctness.

# Part 8 — Failure handling and distributed transaction consistency

| Failure scenario | Expected behavior |
|---|---|
| RabbitMQ unavailable when Product is created | Product and Outbox row commit; dispatcher publishes later |
| Dispatcher crashes after publish but before marking delivered | Event may publish again; consumer idempotency prevents duplicate effects |
| Consumer fails transiently | Retry or delayed redelivery |
| Consumer fails permanently | Message reaches configured error/DLQ handling |
| Duplicate event arrives | Inbox/idempotency prevents repeated business effect |
| Payment fails after stock reservation | Saga releases inventory |
| Service restarts mid-Saga | Persisted Saga state resumes workflow |
| One service is unavailable | Workflow does not report completion prematurely |
| Compensation itself fails | Retry/monitor compensation and expose a failed-Saga state for operations |

## Observability

Log and propagate:

- `MessageId`
- `CorrelationId`
- `ConversationId`
- `CausationId` (where modeled)
- `SagaId`
- Consumer name
- Retry/redelivery count
- Processing duration
- Failure and compensation reason

Do not log credentials, payment secrets or sensitive payloads.

## Recommended implementation order

1. Document and test the current Employee → Product message flow.
2. Add Part 1 and Part 2 routing demonstrations.
3. Add Part 3 DLX/DLQ.
4. Add Part 4 retry and delayed redelivery.
5. Add a separate Order workflow for Part 5.
6. Add Product Bus Outbox for Part 6 to address the existing dual-write.
7. Add consumer idempotency/Consumer Outbox for Part 7.
8. Add fault-injection tests and operational documentation for Part 8.

## Local infrastructure

The current Compose setup defines EmployeeService, ProductService, Redis and RabbitMQ Management. The override maps:

- RabbitMQ AMQP: `localhost:5672`
- RabbitMQ Management UI: `http://localhost:15672`
- Redis: `localhost:6379`
- EmployeeService: host ports `8080`/`8081`
- ProductService: host ports `8082`/`8083`

When services run inside Compose, use `messagebroker:5672` and `distributedcache:6379`. When running services from Visual Studio on the host, use `localhost:5672` and `localhost:6379`.

## Version baseline

The repository currently targets .NET 10 and references `MassTransit` and `MassTransit.RabbitMQ` version `8.3.6` in the shared messaging project. Keep MassTransit packages aligned. Confirm package compatibility before adding Saga persistence or Outbox packages.

## Definition of done

- Each exchange type has a working publish/routing test.
- DLQ captures rejected/expired test messages.
- Retry and delayed redelivery show bounded attempts.
- Saga state persists and compensation is demonstrated.
- Product data and Outbox record commit atomically.
- Duplicate consumer delivery does not duplicate the business effect.
- Failure scenarios are repeatable and documented.
- Docker Compose starts the local dependencies and services.
