
using product_scraper.Dtos;
using product_scraper.Models;
using product_scraper.Repositories;
using Serilog;

namespace product_scraper.Services;

public class FilterService : IFilterService
{
    private readonly IRepository repository;
    private readonly EmailService emailService;

    public FilterService(IRepository repository, EmailService emailService)
    {
        this.repository = repository;
        this.emailService = emailService;
    }

    public async Task<List<MercariListingDto>> FilterAllUnemailedListings()
    {
        List<MercariListingDto> flaggedListings = new();
        List<int> flaggedIds = new();

        var allListings = await repository.GetRecentUnemailedListings();
        var filters = await repository.GetAllFilterCriteria();

        foreach (var item in allListings)
        {
            foreach (var filter in filters)
            {
                var keywordsLower = filter.Keywords?.Select(k => k.ToLower());
                var descriptionLower = item.Description.ToLower();

                if (keywordsLower != null && filter.Keywords.All(keyword => item.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                     && item.Price >= filter.MinPrice && item.Price <= filter.MaxPrice)
                {
                    flaggedListings.Add(new MercariListingDto
                    {
                        Description = item.Description,
                        Price = item.Price,
                        Url = item.Url.StartsWith("jp.mercari.com") ? item.Url : "jp.mercari.com" + item.Url,
                        ImgUrl = item.ImgUrl,
                        Filter = filter
                    });
                    flaggedIds.Add(item.Id);
                }
            }

        }
        // Flag them as emailed in the database.
        await repository.MarkListingsAsEmailed(flaggedIds);
        Log.Information("{total} items flagged", flaggedListings.Count); 
        return flaggedListings;
    }
}
