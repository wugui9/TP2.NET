#!/usr/bin/env dotnet-script
// 查看用户密码
// 运行: dotnet script ViewUsers.csx

#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 9.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.Design, 9.0.0"

using Microsoft.EntityFrameworkCore;

// 简化的用户模型
public class User
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public class SimpleDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Gauniv.WebServer/gauniv.db");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("AspNetUsers");
    }
}

// 查询数据
using var db = new SimpleDbContext();

Console.WriteLine("=================================");
Console.WriteLine("    用户密码查看器");
Console.WriteLine("=================================\n");

Console.WriteLine("👤 用户列表:");
Console.WriteLine("─────────────────────────────────");
var users = db.Users.ToList();
foreach (var user in users)
{
    Console.WriteLine($"邮箱: {user.Email}");
    Console.WriteLine($"姓名: {user.FirstName} {user.LastName}");
    Console.WriteLine($"明文密码: {user.Password}");
    Console.WriteLine($"加密密码: {(string.IsNullOrEmpty(user.PasswordHash) ? "(无)" : user.PasswordHash.Substring(0, Math.Min(50, user.PasswordHash.Length)) + "...")}");
    Console.WriteLine($"注册时间: {user.RegisteredAt:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine("─────────────────────────────────");
}

Console.WriteLine($"\n总计: {users.Count} 个用户\n");
