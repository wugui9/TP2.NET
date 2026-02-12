using Microsoft.Data.Sqlite;

var connectionString = "Data Source=../Gauniv.WebServer/gauniv.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

var command = connection.CreateCommand();
command.CommandText = @"
    SELECT Email, FirstName, LastName, Password, 
           SUBSTR(PasswordHash, 1, 50) as PasswordHashPreview
    FROM AspNetUsers";

using var reader = command.ExecuteReader();

Console.WriteLine("=================================");
Console.WriteLine("    User Password Viewer");
Console.WriteLine("=================================\n");

while (reader.Read())
{
    Console.WriteLine($"Email: {reader.GetString(0)}");
    Console.WriteLine($"Name: {reader.GetString(1)} {reader.GetString(2)}");
    Console.WriteLine($"Plain Password: {reader.GetString(3)}");
    Console.WriteLine($"Hashed Password: {reader.GetString(4)}...");
    Console.WriteLine("─────────────────────────────────");
}
