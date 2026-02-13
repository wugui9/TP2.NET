namespace Gauniv.WpfClient.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public long Size { get; set; }
    public string? FileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOwned { get; set; }

    public List<string> CategoryNames { get; set; } = new();

    public List<Category> Categories { get; set; } = new();
    public string CategoryDisplay =>
        CategoryNames.Count > 0
            ? string.Join(", ", CategoryNames)
            : Categories.Count > 0
                ? string.Join(", ", Categories.Select(c => c.Name))
                : string.Empty;
}
public class GameListResponse
{
    public int Total { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
    public List<Game> Games { get; set; } = new();
}
