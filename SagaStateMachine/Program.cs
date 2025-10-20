using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Context;
using SagaStateMachine.StateInstance;
using SagaStateMachine.StateMachines;
using Shared.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(options =>
    {
        options.AddDbContext<DbContext, OrderStateDbContext>((provider  ,_builder) =>
        {
            _builder.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
        });
    });


    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ"]);
        //State machine config
        cfg.ReceiveEndpoint(RabbitMQSettings.StateMachineQueue, e => e.ConfigureSaga<OrderStateInstance>(ctx));
    });
});

var host = builder.Build();
host.Run();
