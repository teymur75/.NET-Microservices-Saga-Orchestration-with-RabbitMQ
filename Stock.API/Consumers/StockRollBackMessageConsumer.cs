using MassTransit;
using Shared.Message;
using M=Stock.API.Models;
using Stock.API.Services;
using MongoDB.Driver;

namespace Stock.API.Consumers
{
    public class StockRollBackMessageConsumer(MongoDBService _mongoDb) : IConsumer<StockRollBackMessage>
    {
        public async Task Consume(ConsumeContext<StockRollBackMessage> context)
        {
            var stockCollection = _mongoDb.GetCollection<M.Stock>();
            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await (await stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                stock.Count += orderItem.Count;
                await stockCollection.FindOneAndReplaceAsync(s => s.ProductId == orderItem.ProductId, stock);
            }
        }
    }
}
