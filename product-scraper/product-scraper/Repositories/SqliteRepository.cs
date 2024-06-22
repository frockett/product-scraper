using product_scraper.Data;
using product_scraper.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Serilog;

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
                Log.Error(ex.Message);
                transaction.Rollback();
            }
        }
    }

    public async Task AddUrl(UrlToScrape url)
    {
        try
        {
            await context.Urls.AddAsync(url);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"An error occured adding the url {url.Url}. Exception: {ex.Message}");
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
            Log.Error(ex.Message);
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

        Log.Information($"There are {listings.Count} listings total");

        return listings;
    }

    public async Task<HashSet<string>> LoadExistingUrlHashes()
    {
        var urlHashes = await context.MercariListings.Select(l => l.UrlHash).ToListAsync();
        return new HashSet<string>(urlHashes);
    }

    public async Task UpdateUrlHashes()
    {
        try
        {
            var batchSize = 100;
            int numberOfLisitingsProcessed;
            List<MercariListing> listings;
            do
            {
                numberOfLisitingsProcessed = 0;

                listings = await context.MercariListings
                    .Where(l => l.UrlHash == null)
                    .OrderBy(l => l.Id)
                    .Take(batchSize)
                    .ToListAsync();

                foreach (var listing in listings)
                {
                    listing.UrlHash = ComputeSha256Hash(listing.Url);
                    numberOfLisitingsProcessed++;
                }
                await context.SaveChangesAsync();
            } while (numberOfLisitingsProcessed > 0);
        }
        catch (Exception ex)
        {
            Log.Error($"Error updating URL hashes: {ex.Message}");
        }
    }

    public string ComputeSha256Hash(string rawUrl)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawUrl));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public async Task<List<UrlToScrape>> GetActiveUrls()
    {
        return await context.Urls.Where(u => u.Active == true).ToListAsync();  
    }
    public async Task<List<UrlToScrape>> GetAllUrls()
    {
        return await context.Urls.ToListAsync();
    }

    public async Task<List<MercariListing>> GetRecentUnemailedListings()
    {
        var timeOffset = DateTime.UtcNow.AddHours(-1); // Arbitrary magic number, adjust as needed, 1 hour seems like it'll always get the new listings and keep the read small
        return await context.MercariListings
                            .Where(l => !l.IsEmailed && l.CreatedAt >= timeOffset).ToListAsync();
    }

    public async Task ToggleUrlActiveStatus(int urlId)
    {
        var urlEntity = await context.Urls.FindAsync(urlId);
        if (urlEntity != null)
        {
            urlEntity.Active = !urlEntity.Active;
            await context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException($"No URL found with ID: {urlId}");
        }
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
            Log.Error(ex.Message);
        }
    }

    public async Task<List<FilterCriteria>> GetAllFilterCriteria()
    {
        var filterCriteria = await context.FilterCriteria.ToListAsync();
        return filterCriteria;
    }

    public async Task ResetEmailFlags()
    {
        try
        {
            var flaggedListings = await context.MercariListings.Where(x => x.IsEmailed).ToListAsync();

            foreach (var listing in flaggedListings)
            {
                listing.IsEmailed = false;
            }
            await context.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            Log.Error($"Database operation failed. {ex.Message}");
        }
    }

    public Task<List<User>>? GetAllUsers()
    {
        try
        {
            var users = context.Users.ToListAsync();
            return users;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            return null;
        }
    }

    public async Task DeleteOldListings()
    {
        try
        {
            DateTime cutoffDate = DateTime.UtcNow.AddDays(-30);
            var oldListings = await context.MercariListings.Where(x => x.CreatedAt <= cutoffDate).ToListAsync();
            context.MercariListings.RemoveRange(oldListings);
            await context.SaveChangesAsync();
        }
        catch
        {
            throw;
        }
    }
}
