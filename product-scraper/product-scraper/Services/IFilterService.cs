using product_scraper.Dtos;

namespace product_scraper.Services
{
    public interface IFilterService
    {
        Task<List<MercariListingDto>> FilterAllUnemailedListings();
    }
}