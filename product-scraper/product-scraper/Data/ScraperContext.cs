using Microsoft.EntityFrameworkCore;
using product_scraper.Models;
using System.Globalization;

namespace product_scraper.Data;

public class ScraperContext : DbContext
{
    public ScraperContext(DbContextOptions<ScraperContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MercariListing>()
            .Property(e => e.CreatedAt)
            .HasConversion(
                v => v.ToString("yyyy-MM-dd HH:mm:ss"), 
                v => DateTime.Parse(v, CultureInfo.InvariantCulture)
            );
    }

    public virtual DbSet<MercariListing> MercariListings { get; set; }
    public virtual DbSet<FilterCriteria> FilterCriteria { get; set; }
    public virtual DbSet<UrlToScrape> Urls { get; set;}
    public virtual DbSet<User> Users { get; set; }
}
