using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Worker.Consumers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddMassTransit(x =>
        {
            // Register both consumers
            x.AddConsumer<EmailConsumer>();
            x.AddConsumer<SmsConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(ctx.Configuration.GetConnectionString("RabbitMq") ?? "localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // Each consumer gets its own durable queue
                cfg.ReceiveEndpoint("booking-confirmed-email", e =>
                {
                    e.ConfigureConsumer<EmailConsumer>(context);
                });

                cfg.ReceiveEndpoint("booking-confirmed-sms", e =>
                {
                    e.ConfigureConsumer<SmsConsumer>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();
