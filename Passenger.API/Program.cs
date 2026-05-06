using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Passenger.API.Data;
using Passenger.API.Validators;
using Serilog;
using PassengerModel = Passenger.API.Models.Passenger;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.UseUrls($"http://*:{port}");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/passenger-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Database
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(dbConnectionString) && (dbConnectionString.StartsWith("postgres://") || dbConnectionString.StartsWith("postgresql://")))
{
    var uri = new Uri(dbConnectionString);
    var userInfo = uri.UserInfo.Split(':');
    dbConnectionString = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.LocalPath.TrimStart('/')};Pooling=true;";
}
builder.Services.AddDbContext<PassengerDbContext>(options =>
    options.UseNpgsql(dbConnectionString ?? ""));

// FluentValidation
builder.Services.AddScoped<IValidator<PassengerModel>, PassengerValidator>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PassengerDbContext>();
    db.Database.Migrate();
}

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





