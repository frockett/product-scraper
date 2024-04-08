
using product_scraper.Dtos;
using product_scraper.Models;
using product_scraper.Repositories;

namespace product_scraper.Services;

public class FilterService
{
    private readonly IRepository repository;

    public FilterService(IRepository repository)
    {
        this.repository = repository;
    }

    public async Task<List<MercariListingDto>> FilterNewListings()
    {
        List<MercariListingDto> flaggedListings = new();

        var allListings = await repository.GetUnemailedListings();
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
                }
            }

        }

        foreach (var listing in flaggedListings)
        {
            Console.WriteLine($"Keyword matched: {String.Join(", ", listing.Filter.Keywords)}");
            Console.WriteLine($"Description: {listing.Description}, Price: {listing.Price}, Url: {listing.Url}");
        }

        return flaggedListings;
    }
}
