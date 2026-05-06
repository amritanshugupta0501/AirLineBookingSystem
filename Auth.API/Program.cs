using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/auth-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Configure Authentication: Cookies (for Google OAuth Challenge) + Google
builder.Services.AddAuthentication(options =>
{
    // The default scheme for the API is generally JWT or Cookie depending on the flow.
    // For external logins, we use cookies to hold the correlation state.
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "dummy_client_id_for_now";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "dummy_secret_for_now";
    
    // We override the callback path if needed, but default is /signin-google which Yarp correctly matches
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use Serilog request logging
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

// Map Health Checks
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();


