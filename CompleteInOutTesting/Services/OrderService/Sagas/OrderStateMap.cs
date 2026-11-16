using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderService.Sagas;

public class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.TransactionId).HasMaxLength(100);
        entity.Property(x => x.ShipmentId).HasMaxLength(100);
        entity.Property(x => x.FailureReason).HasMaxLength(500);
    }
}
