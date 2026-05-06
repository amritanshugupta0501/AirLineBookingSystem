using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
var connUrl = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("Initial conn string: " + connUrl);
builder.Configuration["ConnectionStrings:DefaultConnection"] = "Host=myhost;";
Console.WriteLine("New conn string: " + builder.Configuration.GetConnectionString("DefaultConnection"));
