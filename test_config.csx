using System;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder();
builder.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string> {
    { "ConnectionStrings:DefaultConnection", "postgres://user:pass@host:5432/db" }
});
var config = builder.Build();

var connUrl = config.GetConnectionString("DefaultConnection");
if (connUrl.StartsWith("postgres://")) {
    config["ConnectionStrings:DefaultConnection"] = "Host=myhost;";
}
Console.WriteLine("New conn string: " + config.GetConnectionString("DefaultConnection"));
