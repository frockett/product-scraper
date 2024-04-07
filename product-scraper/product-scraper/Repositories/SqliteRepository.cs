using product_scraper.Data;
using product_scraper.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace product_scraper.Repositories;

public class SqliteRepository : IRepository
{
    private readonly ScraperContext context;

    public SqliteRepository(ScraperContext context)
    {
        this.context = context;
    }

    public async Task<FilterCriteria> AddFilter(FilterCriteria filter)
    {
        await context.FilterCriteria.AddAsync(filter);
        await context.SaveChangesAsync();
        return filter;
    }

    public async Task AddListings(List<MercariListing> listings)
    {
        await context.MercariListings.AddRangeAsync(listings);
        await context.SaveChangesAsync();

        // FOR TESTING
        await GetAllListings();
    }

    public async Task<bool> DeleteFilter(FilterCriteria filter)
    {
        var filterToDelete =  context.FilterCriteria.FirstOrDefaultAsync(f => f.Id == filter.Id);

        try
        {
            if (filterToDelete != null)
            {
                context.FilterCriteria.Remove(filter);
                await context.SaveChangesAsync();
            }
            else return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

        return true;
    }

    public async Task<MercariListing> FetchMostRecentListing()
    {
        throw new NotImplementedException();
    }

    public async Task<List<MercariListing>> GetAllListings()
    {
        // FOR TESTING
        var listings = await context.MercariListings.ToListAsync();

        Console.Write($"There are {listings.Count} listings total");

/*        foreach (var listing in listings)
        {
            Console.WriteLine($"Desc: {listing.Description} // Price: {listing.Price.ToString()} // URL: {listing.Url} // Scraped At: {listing.CreatedAt}");
        }
        Console.WriteLine("Done!");

        await RemoveOldListings();*/
        return listings;
    }

    public async Task RemoveOldListings()
    {
        // ATTN! CURRENTLY DELETES ALL FOR TESTING PURPOSES! FIX LATER!!
        var listings = await GetAllListings();
        context.MercariListings.RemoveRange(listings);
        await context.SaveChangesAsync();
        Console.WriteLine("Deleted!");
    }
}
