using product_scraper.Models;

namespace product_scraper.Repositories;

public interface IRepository
{
    public Task AddListings(List<MercariListing> listings);
    public Task RemoveOldListings();
    public Task<MercariListing> FetchMostRecentListing();
    public Task<List<MercariListing>> GetAllListings();
    public Task<FilterCriteria> AddFilter(FilterCriteria filter);
    public Task<bool> DeleteFilter(FilterCriteria filter);
}
