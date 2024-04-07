using Microsoft.Playwright;
using product_scraper.Models;
using product_scraper.Repositories;

namespace product_scraper;

public class ScraperService
{
    private readonly IRepository repository;
    private List<MercariListing> listings = new List<MercariListing>();
    private HashSet<string> uniqueLinks = new HashSet<string>();
    private int newUniqueLinks = 0;
    private int repeatThreshold = 3;
    private int repeatCount = 0;

    public ScraperService(IRepository repository)
    {
        this.repository = repository;
    }

    public async Task StartScraping()
    {
        List<string> Urls = new List<string> { "https://jp.mercari.com/search?order=desc&sort=created_time&category_id=20",
                                               "https://jp.mercari.com/search?keyword=balenciaga&sort=created_time&order=desc&category_id=20",
                                               "https://jp.mercari.com/search?keyword=hermes&sort=created_time&order=desc&category_id=20",
                                               "https://jp.mercari.com/search?keyword=gaultier&sort=created_time&order=desc&category_id=20" ,
                                               "https://jp.mercari.com/search?keyword=chanel&sort=created_time&order=desc&category_id=20",
                                               "https://jp.mercari.com/search?keyword=prada&sort=created_time&order=desc&category_id=20" ,
                                               "https://jp.mercari.com/search?keyword=dior&sort=created_time&order=desc&category_id=20" };



        foreach (string Url in Urls)
        {
            await ScrapeSite(Url);
        }
    }

    public async Task ScrapeSite(string Url)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false, Args = new[] { "--start-maximized" }, SlowMo = 50 });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
            Locale = "ja-JP"
        });

        await context.AddCookiesAsync(new[]
        {
            new Cookie
            {
                Name = "country_code",
                Value = "TW",
                Domain = "jp.mercari.com",
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(30).ToUnixTimeSeconds()
            }
        });

        var oldListings = await repository.GetAllListings();

        foreach (var listing in oldListings)
        {
            uniqueLinks.Add(listing.Url);
        }

        // Do the scraping
        try
        {
            var page = await context.NewPageAsync();
            await page.GotoAsync("https://jp.mercari.com/search?order=desc&sort=created_time&category_id=20");

            do
            {
                Console.WriteLine("Starting the loop again! Woohoo!");
                await page.WaitForSelectorAsync("li[data-testid='item-cell']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

                // Scroll to end of page to load all listings
                await Task.Delay(new Random().Next(500, 2000));
                Console.WriteLine("I've waited! Now I'm going to scroll!");
                await ScrollToEnd(page);
                Console.WriteLine("I finished scrolling!");


                var items = await page.QuerySelectorAllAsync("li[data-testid='item-cell']");
                foreach (var item in items)
                {
                    // TODO get the image URL

                    // Cache all of the tags that have desired information
                    var imageContainer = await item.QuerySelectorAsync(".imageContainer__f8ddf3a2");
                    string? description = await imageContainer?.GetAttributeAsync("aria-label");

                    var priceElement = await item.QuerySelectorAsync(".number__6b270ca7");
                    string? price = await priceElement?.InnerTextAsync();

                    var linkElement = await item.QuerySelectorAsync("a[data-location='search_result:newest:body:item_list:item_thumbnail']");
                    string? link = await linkElement?.GetAttributeAsync("href");

                    // Write to console during development
                    Console.WriteLine($"Description: {description}, Price: {price}, Link: {link}");

                    // Randomly move the mouse around
                    var boundingBox = await item.BoundingBoxAsync();
                    if (boundingBox != null)
                    {
                        await page.Mouse.MoveAsync(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2);
                        await Task.Delay(new Random().Next(100, 500)); // Short pause after moving the mouse
                    }
                    await page.Mouse.MoveAsync(new Random().Next(0, 500), new Random().Next(0, 500)); // Adjust dimensions based on expected page size

                    price = price.Replace(",", "");
                    
                    if (uniqueLinks.Contains(link))
                    {
                        repeatCount++;
                        Console.WriteLine($"Current repeats: {repeatCount}");
                        if (repeatCount >= repeatThreshold) break;
                    }
                    else
                    {
                        if (int.TryParse(price, out int parsedPrice))
                        {
                            // All is normal, save the listing
                            listings.Add(new MercariListing { Description = description, Price = parsedPrice, Url = link });
                        }
                        else
                        {
                            // Save listing with dummy price and log the error
                            listings.Add(new MercariListing { Description = description, Price = 0, Url = link });
                            var logMessage = $"Failed to parse price for listing: {description}, Price: {price}, Url: {link}, Time: {DateTime.UtcNow}\n";
                            File.AppendAllText($"failed_parses_{DateTime.UtcNow.Date:yyyyMMdd}.txt", logMessage);
                        }
                        uniqueLinks.Add(link);
                        newUniqueLinks++;
                        repeatCount = 0;
                    }
  

                    // Random delay between actions in ms
                    await Task.Delay(new Random().Next(500, 2000));
                }

                // Go to the next page
                var nextButton = await page.QuerySelectorAsync("[data-testid='pagination-next-button']");
                if (nextButton != null && repeatCount < repeatThreshold)
                {
                    await Task.Delay(new Random().Next(500, 2000));
                    await nextButton.ClickAsync();
                }
                else
                {
                    Console.WriteLine("No next button found!");
                    break;
                }

            } while (repeatCount < repeatThreshold && newUniqueLinks < 700); // 700 is an arbitrary limit, about 7 pages


            await browser.CloseAsync();

            await repository.AddListings(listings);
        }
        catch (Exception ex)
        {
            var logDirectory = "logs";
            var logFilePath = Path.Combine(logDirectory, "error_log.txt");
            var logMessage = $"Error occurred at {DateTime.UtcNow}: {ex.Message}\nStack Trace: {ex.StackTrace}\n";

            Directory.CreateDirectory(logDirectory);

            File.AppendAllText(logFilePath, logMessage);

            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            if (browser != null)
            {
                await browser.CloseAsync();
            }
        }

        Console.Write($"Finished scraping, found {uniqueLinks.Count} unique links!");
    }

    private async Task ScrollToEnd(IPage page)
    {
        await page.EvaluateAsync(@"async () => {
        await new Promise((resolve, reject) => {
            var totalHeight = 0;
            var distance = 100;
            var scrollUpEvery = 5; // Scroll up every 5 scrolls
            var scrollCount = 0;
            var timer = setInterval(() => {
                // Introduce some randomness in scroll distance
                var randDistance = distance + Math.floor(Math.random() * 100) - 50;
                if (scrollCount % scrollUpEvery === 4) {
                    // Occasionally scroll up a bit
                    window.scrollBy(0, -randDistance);
                } else {
                    window.scrollBy(0, randDistance);
                    totalHeight += randDistance;
                }
                scrollCount++;
                if(totalHeight >= document.body.scrollHeight){
                    clearInterval(timer);
                    resolve();
                }
            }, 100 + Math.random() * 200); // Randomize interval as well
        });
    }");
    }

}
