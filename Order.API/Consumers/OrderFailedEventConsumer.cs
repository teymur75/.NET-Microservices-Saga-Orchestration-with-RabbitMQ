using MassTransit;
using Order.API.Contexts;
using Shared.OrderEvents;
using M = Order.API.Models;

namespace Order.API.Consumers
{
    public class OrderFailedEventConsumer(OrderContext _db) : IConsumer<OrderFailedEvent>
    {
        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            M.Order order = await _db.Orders.FindAsync(context.Message.OrderId);
            if (order is not null)
            {
                order.Orderstatus = Enums.OrderStatus.Failed;
                await _db.SaveChangesAsync();
            }
        }
    }
}