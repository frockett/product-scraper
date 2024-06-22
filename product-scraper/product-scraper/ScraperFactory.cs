

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace product_scraper;

public class ScraperFactory : IScraperFactory
{
    private readonly IServiceScopeFactory serviceScopeFactory;

    public ScraperFactory(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public IScraper GetScraper(string url)
    {
        string scraperType = DetermineScraperType(url);

        switch (scraperType)
        {
            case "Mercari":
                return new MercariScraper(serviceScopeFactory);
            default:
                throw new NotSupportedException($"No scraper was found for type: {scraperType}. It might not be supported yet.");
        }
    }

    private string DetermineScraperType(string url)
    {
        if (url.ToLower().Contains("mercari"))
        {
            return "Mercari";
        }
        else return "";
    }
}
