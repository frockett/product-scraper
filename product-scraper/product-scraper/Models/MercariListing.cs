namespace product_scraper.Models;

public class MercariListing
{
    public MercariListing()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    public required string Description { get; set; }
    public int Price { get; set; }
    public required string Url { get; set; }
    public string? imgUrl { get; set; }
    public DateTime CreatedAt { get; private set; }
}
