using MassTransit;
using Shared.OrderEvents;
using M=Stock.API.Models;
using Stock.API.Services;
using MongoDB.Driver;
using Shared.Settings;
using Shared.StockEvents;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer (MongoDBService _mongoDB , ISendEndpointProvider _provider) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResults = new();
            var collection= _mongoDB.GetCollection<M.Stock>();
            foreach (var orderItem in context.Message.OrderItems)
            {
               stockResults.Add(await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count)).AnyAsync());
            }

            var endpoint = await _provider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));

            if (stockResults.TrueForAll(s => s.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    stock.Count-=orderItem.Count;
                    await collection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
                }

                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems=context.Message.OrderItems,
                };

                await endpoint.Send(stockReservedEvent);
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stok azdir"
                };
                await endpoint.Send(stockNotReservedEvent);
            }
        }
    }
}
