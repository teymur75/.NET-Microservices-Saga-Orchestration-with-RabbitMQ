using MassTransit;
using MassTransit.Initializers;
using Shared.PaymentEvents;
using Shared.Settings;

namespace Payment.API.Consumers
{
    public class PaymentStartedEventConsumer(ISendEndpointProvider _provider) : IConsumer<PaymentStartedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            var endPoint =await _provider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));

            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId)
                {
                    
                };
                await endPoint.Send(paymentCompletedEvent);
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Balans azdir",
                    OrderItems = context.Message.OrderItems,
                };
                await endPoint.Send(paymentFailedEvent);
            }
        }
    }
}
