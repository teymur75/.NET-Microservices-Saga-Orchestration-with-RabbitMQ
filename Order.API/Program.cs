using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Contexts;
using M = Order.API.Models;
using Order.API.Viewmodels;
using Order.API.Models;
using Shared.OrderEvents;
using Shared.Settings;
using Order.API.Consumers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCompletedEventConsumer>();
    configurator.AddConsumer<OrderFailedEventConsumer>();
    
    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ"]);
        cfg.ReceiveEndpoint(RabbitMQSettings.Order_OrderCompletedEventQueue, e => e.ConfigureConsumer<OrderCompletedEventConsumer>(ctx));
        cfg.ReceiveEndpoint(RabbitMQSettings.Order_OrderFailedEventQueue, e => e.ConfigureConsumer<OrderFailedEventConsumer>(ctx));
    });
});

builder.Services.AddDbContext<OrderContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/create-order", async ( VM_CreateOrder model , OrderContext _db , IPublishEndpoint _publisher) =>
{
    M.Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.Now,
        Orderstatus = Order.API.Enums.OrderStatus.Suspended,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new OrderItem()
        {
            Price = oi.Price,
            Count = oi.Count,
            ProcutId=oi.ProductId
        }).ToList()
    };

    await _db.AddAsync(order);
    await _db.SaveChangesAsync();

    OrderStartedEvent orderStartedEvent = new()
    {
        BuyerId = model.BuyerId,
        OrderId = order.Id,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new Shared.Message.OrderItemMessage()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId
        }).ToList()

    };

    await _publisher.Publish(orderStartedEvent);

});


app.Run();
