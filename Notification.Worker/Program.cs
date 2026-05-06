using Serilog;
using MassTransit;
using Notification.Worker.Consumers;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/notification-worker-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<BookingConfirmedEventConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitUrl = builder.Configuration.GetConnectionString("RabbitMq");
            if (!string.IsNullOrEmpty(rabbitUrl) && (rabbitUrl.StartsWith("amqp://") || rabbitUrl.StartsWith("amqps://")))
            {
                var uri = new Uri(rabbitUrl);
                var userInfo = uri.UserInfo.Split(':');
                cfg.Host(uri.Host, uri.LocalPath, h =>
                {
                    h.Username(userInfo[0]);
                    h.Password(Uri.UnescapeDataString(userInfo[1]));
                    if (rabbitUrl.StartsWith("amqps://"))
                        h.UseSsl(s => { });
                });
            }
            else
            {
                cfg.Host(rabbitUrl ?? "localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            }

            cfg.ReceiveEndpoint("notification-queue", e =>
            {
                e.ConfigureConsumer<BookingConfirmedEventConsumer>(context);
            });
        });
    });

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
