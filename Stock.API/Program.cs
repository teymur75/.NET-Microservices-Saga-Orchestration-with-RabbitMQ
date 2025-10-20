using MassTransit;
using M = Stock.API.Models;
using Stock.API.Services;
using MongoDB.Driver;
using Stock.API.Consumers;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<StockRollBackMessageConsumer>();
    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ"]);
        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(ctx));
        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_RollBackMessageQueue, e => e.ConfigureConsumer<StockRollBackMessageConsumer>(ctx));
    });
});

builder.Services.AddSingleton<MongoDBService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDBService>();
    var collection = mongoDbService.GetCollection<M.Stock>();

    var allStocks = await collection.Find(_ => true).ToListAsync();

    if (!allStocks.Any())
    {
        await collection.InsertOneAsync(new M.Stock() { ProductId = 1, Count = 200 });
        await collection.InsertOneAsync(new M.Stock() { ProductId = 2, Count = 300 });
        await collection.InsertOneAsync(new M.Stock() { ProductId = 3, Count = 400 });
        await collection.InsertOneAsync(new M.Stock() { ProductId = 4, Count = 500 });
    }
}

app.Run();
