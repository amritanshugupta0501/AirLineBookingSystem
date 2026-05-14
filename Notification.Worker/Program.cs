using Serilog;
using MassTransit;
using Notification.Worker.Consumers;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/notification-worker-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    // Use WebApplication so we can bind to PORT and satisfy Render's health check
    var builder = WebApplication.CreateBuilder(args);

    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://*:{port}");

    builder.Host.UseSerilog();

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

                // Pass the full URI so MassTransit picks up host, port AND virtual host correctly
                cfg.Host(uri, h =>
                {
                    if (userInfo.Length >= 1) h.Username(userInfo[0]);
                    if (userInfo.Length >= 2) h.Password(Uri.UnescapeDataString(userInfo[1]));
                    if (rabbitUrl.StartsWith("amqps://"))
                        h.UseSsl(s => { });
                });
            }
            else
            {
                cfg.Host("localhost", "/", h =>
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

    var app = builder.Build();

    // Minimal health endpoint so Render's port scan succeeds
    app.MapGet("/health", () => Results.Ok("Notification Worker running"));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
