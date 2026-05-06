using Serilog;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/payment-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MassTransit/RabbitMQ
builder.Services.AddMassTransit(x =>
{
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
    });
});


// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");

app.UseAuthorization();
app.MapControllers();

app.Run();

