using product_scraper.Models;

namespace product_scraper.Repositories;

public interface IRepository
{
    public Task AddListings(List<MercariListing> listings);
    public Task AddUrl(UrlToScrape url);
    public Task RemoveOldListings();
    public Task<MercariListing> FetchMostRecentListing();
    public Task<List<MercariListing>> GetAllListings();
    public Task<HashSet<string>> LoadExistingUrlHashes();
    public Task UpdateUrlHashes();
    public string ComputeSha256Hash(string rawUrl);
    public Task<List<UrlToScrape>> GetActiveUrls();
    public Task<List<UrlToScrape>> GetAllUrls();
    public Task<List<MercariListing>> GetRecentUnemailedListings();
    public Task MarkListingsAsEmailed(List<int> listingIds);
    public Task ToggleUrlActiveStatus(int urlId);
    public Task<FilterCriteria> AddFilter(FilterCriteria filter);
    public Task<bool> DeleteFilter(FilterCriteria filter);
    public Task<List<FilterCriteria>> GetAllFilterCriteria();
    public Task ResetEmailFlags();
    public Task<List<User>>? GetAllUsers();
}
