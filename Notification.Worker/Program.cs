using Notification.Worker;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

// Setup MassTransit
builder.Services.AddMassTransit(x =>
{
    // Register consumer
    x.AddConsumer<PaymentConfirmedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Automatically configure the receive endpoints
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
