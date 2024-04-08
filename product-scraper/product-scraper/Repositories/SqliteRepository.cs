using product_scraper.Data;
using product_scraper.Models;
using Microsoft.EntityFrameworkCore;

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
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {
                await context.MercariListings.AddRangeAsync(listings);
                await context.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                transaction.Rollback();
            }
        }
    }

    public async Task<bool> DeleteFilter(FilterCriteria filter)
    {
        var filterToDelete =  await context.FilterCriteria.FirstOrDefaultAsync(f => f.Id == filter.Id);

        try
        {
            if (filterToDelete != null)
            {
                context.FilterCriteria.Remove(filterToDelete);
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
        var listings = await context.MercariListings.ToListAsync();

        Console.Write($"There are {listings.Count} listings total");

        return listings;
    }
    public async Task<List<UrlsToScrape>> GetActiveUrls()
    {
        return await context.Urls.Where(u => u.Active == true).ToListAsync();  
    }
    public async Task<List<UrlsToScrape>> GetAllUrls()
    {
        return await context.Urls.ToListAsync();
    }

    public async Task<List<MercariListing>> GetUnemailedListings()
    {
        return await context.MercariListings.Where(l => !l.IsEmailed).ToListAsync();
    }

    public async Task MarkListingsAsEmailed(List<int> listingIds)
    {
        try
        {
            var listingsToUpdate = await context.MercariListings.Where(l => listingIds.Contains(l.Id)).ToListAsync();

            foreach (var listing in listingsToUpdate)
            {
                listing.IsEmailed = true;
            }

            await context.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task RemoveOldListings()
    {
        // ATTN! CURRENTLY DELETES ALL FOR TESTING PURPOSES! FIX LATER!!
        var listings = await GetAllListings();
        context.MercariListings.RemoveRange(listings);
        await context.SaveChangesAsync();
        Console.WriteLine("Deleted!");
    }

    public async Task<List<FilterCriteria>> GetAllFilterCriteria()
    {
        var filterCriteria = await context.FilterCriteria.ToListAsync();
        return filterCriteria;
    }
}
