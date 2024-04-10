using Microsoft.EntityFrameworkCore;
using product_scraper.Models;

namespace product_scraper.Data;

public class ScraperContext : DbContext
{
    public ScraperContext(DbContextOptions<ScraperContext> options) : base(options)
    {
    }

    public virtual DbSet<MercariListing> MercariListings { get; set; }
    public virtual DbSet<FilterCriteria> FilterCriteria { get; set; }
    public virtual DbSet<UrlToScrape> Urls { get; set;}
    public virtual DbSet<User> Users { get; set; }
}
