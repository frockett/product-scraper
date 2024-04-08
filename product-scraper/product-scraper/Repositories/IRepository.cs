using product_scraper.Models;

namespace product_scraper.Repositories;

public interface IRepository
{
    public Task AddListings(List<MercariListing> listings);
    public Task RemoveOldListings();
    public Task<MercariListing> FetchMostRecentListing();
    public Task<List<MercariListing>> GetAllListings();
    public Task<List<MercariListing>> GetUnemailedListings();
    public Task MarkListingsAsEmailed(List<int> listingIds);
    public Task<FilterCriteria> AddFilter(FilterCriteria filter);
    public Task<bool> DeleteFilter(FilterCriteria filter);
    public Task<List<FilterCriteria>> GetAllFilterCriteria();
}
