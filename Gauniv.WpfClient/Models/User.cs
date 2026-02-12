namespace Gauniv.WpfClient.Models;

public class User
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }

    public string DisplayName => string.IsNullOrEmpty(FirstName) ? Email : $"{FirstName} {LastName}";
}
