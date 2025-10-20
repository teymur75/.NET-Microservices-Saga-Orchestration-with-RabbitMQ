using MassTransit;
using SagaStateMachine.StateInstance;
using Shared.Message;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.Settings;
using Shared.StockEvents;

namespace SagaStateMachine.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }

        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }

        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);

            Event(() => OrderStartedEvent, x =>
            {
                x.CorrelateBy<int>(s => s.OrderId, m => m.Message.OrderId);
                x.SelectId(_ => NewId.NextGuid());  // Guid.NewGuid() da olar, amma NewId daha uyğundur
                x.InsertOnInitial = true;           // <-- BUNU ƏLAVƏ EDİN
            });


            Event(() => StockReservedEvent,
              orderStateInstance => orderStateInstance.CorrelateById( @event => @event.Message.CorrelationId));

            Event(() => StockNotReservedEvent,
             orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => PaymentCompletedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => PaymentFailedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.CreatedDate = DateTime.Now;
                })
                .TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems,
                }));

            During(OrderCreated, When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"),
                context=>new PaymentStartedEvent(context.Instance.CorrelationId)
                {
                    TotalPrice = context.Instance.TotalPrice,
                    OrderItems = context.Data.OrderItems,
                }),
                When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                context=> new OrderFailedEvent()
                {
                    OrderId=context.Instance.OrderId,
                    Message = context.Data.Message
                }));

            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
                context => new OrderCompletedEvent()
                {
                    OrderId=context.Instance.OrderId
                })
                .Finalize(),
                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                context=>new OrderFailedEvent()
                {
                    Message=context.Data.Message,
                    OrderId=context.Instance.OrderId
                })
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollBackMessageQueue}"),
                context => new StockRollBackMessage()
                {
                    OrderItems=context.Data.OrderItems
                }));

            SetCompletedWhenFinalized();
        }

    }
}
