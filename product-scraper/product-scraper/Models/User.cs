
namespace product_scraper.Models;

public class User
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public required string Email { get; set; }
}
