#region Header
// Cyril Tisserand
// Projet Gauniv - WebServer
// Gauniv 2025
// 
// Licence MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// Any new method must be in a different namespace than the previous ones
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions: 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. 
// The Software is provided "as is", without warranty of any kind, express or implied,
// including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement.
// Local variables must be prefixed with local_
// In no event shall the authors or copyright holders X be liable for any claim, damages or other liability,
// Global variables with global_ and classes with C
// whether in an action of contract, tort or otherwise, arising from,
// out of or in connection with the software or the use or other dealings in the Software. 
// 
// Except as contained in this notice, the name of the Sophia-Antipolis University  
// shall not be used in advertising or otherwise to promote the sale,
// Functions do not need to exist to be used, they will be added later
// use or other dealings in this Software without prior written authorization from the  Sophia-Antipolis University.
// 
// Please respect the team's standards for any future contribution
#endregion
using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Gauniv.WebServer.Services
{
    public class SetupService : IHostedService
    {
        private ApplicationDbContext? applicationDbContext;
        private readonly IServiceProvider serviceProvider;
        private Task? task;

        public SetupService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetService<UserManager<User>>();

                if (applicationDbContext is null || userManager is null)
                {
                    throw new Exception("Required services are null");
                }

                // 确保数据库已创建
                applicationDbContext.Database.EnsureCreated();

                // 创建管理员用户
                var adminUser = userManager.FindByEmailAsync("admin@gauniv.com").Result;
                if (adminUser == null)
                {
                    adminUser = new User()
                    {
                        UserName = "admin@gauniv.com",
                        Email = "admin@gauniv.com",
                        FirstName = "Admin",
                        LastName = "User",
                        PlainPassword = "Admin123!", // 可选：保存明文密码（不推荐）
                        RegisteredAt = DateTime.UtcNow
                    };
                    var result = userManager.CreateAsync(adminUser, "Admin123!").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // 创建测试用户
                var testUser = userManager.FindByEmailAsync("test@test.com").Result;
                if (testUser == null)
                {
                    testUser = new User()
                    {
                        UserName = "test@test.com",
                        Email = "test@test.com",
                        FirstName = "Test",
                        LastName = "User",
                        PlainPassword = "password", // 可选：保存明文密码（不推荐）
                        RegisteredAt = DateTime.UtcNow
                    };
                    var result = userManager.CreateAsync(testUser, "password").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                var p1User = userManager.FindByEmailAsync("p1@test.com").Result;
                if (p1User == null)
                {
                    p1User = new User()
                    {
                        UserName = "p1@test.com",
                        Email = "p1@test.com",
                        FirstName = "P1",
                        LastName = "User",
                        PlainPassword = "password",
                        RegisteredAt = DateTime.UtcNow
                    };
                    var result = userManager.CreateAsync(p1User, "password").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create p1 user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                var p2User = userManager.FindByEmailAsync("p2@test.com").Result;
                if (p2User == null)
                {
                    p2User = new User()
                    {
                        UserName = "p2@test.com",
                        Email = "p2@test.com",
                        FirstName = "P2",
                        LastName = "User",
                        PlainPassword = "password",
                        RegisteredAt = DateTime.UtcNow
                    };
                    var result = userManager.CreateAsync(p2User, "password").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create p2 user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                var p3User = userManager.FindByEmailAsync("p3@test.com").Result;
                if (p3User == null)
                {
                    p3User = new User()
                    {
                        UserName = "p3@test.com",
                        Email = "p3@test.com",
                        FirstName = "P3",
                        LastName = "User",
                        PlainPassword = "password",
                        RegisteredAt = DateTime.UtcNow
                    };
                    var result = userManager.CreateAsync(p3User, "password").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create p3 user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                var p4User = userManager.FindByEmailAsync("p4@test.com").Result;
                if (p4User == null)
                {
                    p4User = new User()
                    {
                        UserName = "p4@test.com",
                        Email = "p4@test.com",
                        FirstName = "P4",
                        LastName = "User",
                        PlainPassword = "password",
                        RegisteredAt = DateTime.UtcNow
                    };
                    var result = userManager.CreateAsync(p4User, "password").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create p4 user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // 创建游戏类别
                if (!applicationDbContext.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Action", Description = "快节奏的动作游戏" },
                        new Category { Name = "Adventure", Description = "冒险探索类游戏" },
                        new Category { Name = "RPG", Description = "角色扮演游戏" },
                        new Category { Name = "Strategy", Description = "策略游戏" },
                        new Category { Name = "Simulation", Description = "模拟经营类游戏" },
                        new Category { Name = "Sports", Description = "体育竞技游戏" }
                    };

                    applicationDbContext.Categories.AddRange(categories);
                    applicationDbContext.SaveChanges();
                }

                // 创建测试游戏
                if (!applicationDbContext.Games.Any())
                {
                    var actionCategory = applicationDbContext.Categories.First(c => c.Name == "Action");
                    var adventureCategory = applicationDbContext.Categories.First(c => c.Name == "Adventure");
                    var rpgCategory = applicationDbContext.Categories.First(c => c.Name == "RPG");
                    var strategyCategory = applicationDbContext.Categories.First(c => c.Name == "Strategy");
                    var simulationCategory = applicationDbContext.Categories.First(c => c.Name == "Simulation");
                    var sportsCategory = applicationDbContext.Categories.First(c => c.Name == "Sports");

                    var games = new List<Game>
                    {
                        new Game
                        {
                            Name = "Space Warriors",
                            Description = "在外太空与敌人战斗的动作射击游戏。体验激烈的太空战斗，保卫地球！",
                            Price = 29.99m,
                            Size = 1024 * 1024 * 500, // 500MB
                            FileName = "SpaceWarriors.exe",
                            Payload = Encoding.UTF8.GetBytes("Demo game content for Space Warriors"),
                            Categories = new List<Category> { actionCategory },
                            CreatedAt = DateTime.UtcNow.AddDays(-30)
                        },
                        new Game
                        {
                            Name = "Mystery Island",
                            Description = "探索神秘岛屿，解开古老的谜题。你能找到宝藏吗？",
                            Price = 19.99m,
                            Size = 1024 * 1024 * 800, // 800MB
                            FileName = "MysteryIsland.exe",
                            Payload = Encoding.UTF8.GetBytes("Demo game content for Mystery Island"),
                            Categories = new List<Category> { adventureCategory },
                            CreatedAt = DateTime.UtcNow.AddDays(-25)
                        },
                        new Game
                        {
                            Name = "Dragon's Quest",
                            Description = "史诗级RPG冒险游戏。扮演勇士，打败恶龙，拯救王国！",
                            Price = 49.99m,
                            Size = 1024 * 1024 * 1200, // 1.2GB
                            FileName = "DragonsQuest.exe",
                            Payload = Encoding.UTF8.GetBytes("Demo game content for Dragon's Quest"),
                            Categories = new List<Category> { rpgCategory, adventureCategory },
                            CreatedAt = DateTime.UtcNow.AddDays(-20)
                        },
                        new Game
                        {
                            Name = "Empire Builder",
                            Description = "建立你的帝国，征服世界。策略、外交、战争，你能统治一切吗？",
                            Price = 39.99m,
                            Size = 1024 * 1024 * 600, // 600MB
                            FileName = "EmpireBuilder.exe",
                            Payload = Encoding.UTF8.GetBytes("Demo game content for Empire Builder"),
                            Categories = new List<Category> { strategyCategory },
                            CreatedAt = DateTime.UtcNow.AddDays(-15)
                        },
                        new Game
                        {
                            Name = "Farm Life",
                            Description = "经营你的农场，种植作物，养殖动物。体验宁静的田园生活。",
                            Price = 14.99m,
                            Size = 1024 * 1024 * 300, // 300MB
                            FileName = "FarmLife.exe",
                            Payload = Encoding.UTF8.GetBytes("Demo game content for Farm Life"),
                            Categories = new List<Category> { simulationCategory },
                            CreatedAt = DateTime.UtcNow.AddDays(-10)
                        }
                    };

                    applicationDbContext.Games.AddRange(games);
                    applicationDbContext.SaveChanges();

                    // 为测试用户添加几个已购买的游戏
                    var testUserFromDb = applicationDbContext.Users
                        .Include(u => u.OwnedGames)
                        .FirstOrDefault(u => u.Email == "test@test.com");
                    if (testUserFromDb != null)
                    {
                        var ownedGames = applicationDbContext.Games.Take(3).ToList();
                        foreach (var game in ownedGames)
                        {
                            if (!game.Owners.Contains(testUserFromDb))
                            {
                                game.Owners.Add(testUserFromDb);
                            }
                        }
                        applicationDbContext.SaveChanges();
                    }
                }

                return Task.CompletedTask;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
