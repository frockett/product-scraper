using Microsoft.Playwright;
using product_scraper.Models;
using product_scraper.Repositories;

namespace product_scraper;

public interface IScraper
{
    Task ScrapeSite(IBrowserContext context, IRepository repository, string url);
    Task StartScraping(List<UrlToScrape> urls);
    bool CanHandleUrl(string url);
}