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
Console.WriteLine("    用户密码查看器");
Console.WriteLine("=================================\n");

while (reader.Read())
{
    Console.WriteLine($"邮箱: {reader.GetString(0)}");
    Console.WriteLine($"姓名: {reader.GetString(1)} {reader.GetString(2)}");
    Console.WriteLine($"明文密码: {reader.GetString(3)}");
    Console.WriteLine($"加密密码: {reader.GetString(4)}...");
    Console.WriteLine("─────────────────────────────────");
}
