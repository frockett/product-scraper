
using Microsoft.Extensions.DependencyInjection;
using product_scraper.Models;
using product_scraper.Repositories;

namespace product_scraper.Services;

public class ScraperService
{
    private readonly IScraperFactory scraperFactory;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IRepository repository;

    public ScraperService(IScraperFactory scraperFactory, IServiceScopeFactory serviceScopeFactory, IRepository repository)
    {
        this.scraperFactory = scraperFactory;
        this.scopeFactory = serviceScopeFactory;
        this.repository = repository;
    }

    public async Task StartScraping()
    {
        List<UrlToScrape> urls;
        using (var scope = scopeFactory.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
            urls = await repository.GetActiveUrls();
        }

        var categorizedUrls = new Dictionary<string, List<UrlToScrape>>();

        foreach (var url in urls)
        {
            string scraperKey = await DetermineScraperKey(url.Url);
            if (!categorizedUrls.ContainsKey(scraperKey))
            {
                categorizedUrls[scraperKey] = new List<UrlToScrape>();
            }
            categorizedUrls[scraperKey].Add(url);
        }

        foreach (var key in categorizedUrls.Keys)
        {
            var scraper = scraperFactory.GetScraper(key);
            await scraper.StartScraping(categorizedUrls[key]);
        }
    }

    private async Task<string> DetermineScraperKey(string url)
    {
        if (url.Contains("mercari.com"))
        {
            return "Mercari";
        }
        else
        {
            return "Default";
        }
    }
}
