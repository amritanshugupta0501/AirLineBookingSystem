using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Passenger.API.Data;
using Passenger.API.Validators;
using PassengerModel = Passenger.API.Models.Passenger;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<PassengerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=PassengerDb;Trusted_Connection=True;"));

// FluentValidation
builder.Services.AddScoped<IValidator<PassengerModel>, PassengerValidator>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseAuthorization();
app.MapControllers();

app.Run();
