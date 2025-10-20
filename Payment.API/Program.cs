using MassTransit;
using Payment.API.Consumers;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentStartedEventConsumer>();

    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ"]);
        cfg.ReceiveEndpoint(RabbitMQSettings.Payment_StartedEventQueue, e => e.ConfigureConsumer<PaymentStartedEventConsumer>(ctx));
    });
});

var app = builder.Build();
app.Run();
