
namespace product_scraper.Models;

public class UrlsToScrape
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public bool Active { get; set; } = true;
}
