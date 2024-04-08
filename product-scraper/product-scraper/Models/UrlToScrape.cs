
namespace product_scraper.Models;

public class UrlToScrape
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public bool Active { get; set; } = true;
}
