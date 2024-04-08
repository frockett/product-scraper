namespace product_scraper
{
    public interface IScraperFactory
    {
        IScraper GetScraper(string url);
    }
}