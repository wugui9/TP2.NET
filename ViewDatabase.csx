#!/usr/bin/env dotnet-script
// 使用 Entity Framework 查看数据库
// 运行: dotnet script ViewDatabase.csx

#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 9.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.Design, 9.0.0"

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

// 简化的数据模型
public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? FileName { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SimpleDbContext : DbContext
{
    public DbSet<Game> Games { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Gauniv.WebServer/gauniv.db");
    }
}

// 查询数据
using var db = new SimpleDbContext();

Console.WriteLine("=================================");
Console.WriteLine("    Gauniv Database Viewer");
Console.WriteLine("=================================\n");

Console.WriteLine("📊 游戏列表:");
Console.WriteLine("─────────────────────────────────");
var games = db.Games.Take(10).ToList();
foreach (var game in games)
{
    Console.WriteLine($"ID: {game.Id,-3} | {game.Name,-20} | 价格: ${game.Price,-8:F2} | 文件: {game.FileName}");
}

Console.WriteLine($"\n总计: {db.Games.Count()} 个游戏\n");

Console.WriteLine("📁 分类列表:");
Console.WriteLine("─────────────────────────────────");
var categories = db.Categories.ToList();
foreach (var cat in categories)
{
    Console.WriteLine($"ID: {cat.Id,-3} | {cat.Name,-15} | {cat.Description}");
}

Console.WriteLine($"\n总计: {categories.Count} 个分类\n");
