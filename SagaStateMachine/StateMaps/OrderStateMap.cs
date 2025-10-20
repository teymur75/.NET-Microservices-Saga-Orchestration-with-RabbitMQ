﻿using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SagaStateMachine.StateInstance;

namespace SagaStateMachine.StateMaps
{
    public class OrderStateMap : SagaClassMap<OrderStateInstance>
    {
        protected override void Configure(EntityTypeBuilder<OrderStateInstance> entity, ModelBuilder model)
        {
            entity.Property(x => x.BuyerId).IsRequired();
            entity.Property(x => x.OrderId).IsRequired();
            entity.Property(x => x.BuyerId).HasDefaultValue(0);
        }
    }
}
