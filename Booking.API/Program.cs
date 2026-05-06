using Booking.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var redisUrl = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisUrl) && redisUrl.StartsWith("redis://"))
{
    redisUrl = redisUrl.Substring("redis://".Length);
    if (redisUrl.Contains("@"))
    {
        var parts = redisUrl.Split('@');
        var password = parts[0].Contains(":") ? parts[0].Split(':')[1] : parts[0];
        builder.Configuration["ConnectionStrings:Redis"] = $"{parts[1]},password={password}";
    }
    else
    {
        builder.Configuration["ConnectionStrings:Redis"] = redisUrl;
    }
}

var connUrl = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connUrl) && connUrl.StartsWith("postgres://"))
{
    var uri = new Uri(connUrl);
    var userInfo = uri.UserInfo.Split(':');
    builder.Configuration["ConnectionStrings:DefaultConnection"] = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.LocalPath.TrimStart('/')};Pooling=true;";
}
builder.WebHost.UseUrls($"http://*:{port}");

// Add services to the container.
builder.Services.AddControllers();

// Configure Database
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=BookingDb;Username=postgres;Password=postgres"));

// Add Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

// Add Health Checks
builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-apply migrations on startup — creates BookingDb if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map Health Checks
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();





