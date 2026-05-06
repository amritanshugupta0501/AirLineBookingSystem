using System;
using Npgsql;
try {
    var b = new NpgsqlConnectionStringBuilder("postgres://user:pass@host/db");
} catch (Exception ex) {
    Console.WriteLine("Test 1: " + ex.Message);
}
try {
    var b = new NpgsqlConnectionStringBuilder("  postgres://user:pass@host/db");
} catch (Exception ex) {
    Console.WriteLine("Test 2: " + ex.Message);
}
try {
    var b = new NpgsqlConnectionStringBuilder("");
} catch (Exception ex) {
    Console.WriteLine("Test 3: " + ex.Message);
}
