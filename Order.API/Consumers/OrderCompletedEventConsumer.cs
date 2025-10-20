using MassTransit;
using Order.API.Contexts;
using Shared.OrderEvents;
using M = Order.API.Models;

namespace Order.API.Consumers
{
    public class OrderCompletedEventConsumer (OrderContext _db) : IConsumer<OrderCompletedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            M.Order order = await _db.Orders.FindAsync(context.Message.OrderId);
            if(order is not null)
            {
                order.Orderstatus = Enums.OrderStatus.Completed;
                await _db.SaveChangesAsync();
            }
        }
    }
}
