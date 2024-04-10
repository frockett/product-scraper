using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using product_scraper.Models;
using product_scraper.Repositories;

namespace product_scraper;

public class MercariScraper : IScraper
{
    //private readonly IRepository repository;
    private readonly IServiceScopeFactory scopeFactory;
    private HashSet<string> uniqueLinks = new HashSet<string>();
    private List<string> userAgents = new List<string>
        {
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_5) AppleWebKit/605.1.15 (KHTML, like Gecko) Chrome/123.0.0.0 Version/11.1.1 Safari/605.1.15",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.6903.32 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.02",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 GLS/100.10.9571.96",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 PTST/240304.190241",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36"
        };
    private int repeatLimit = 10;
    private int newLinkLimit = 500;

    public MercariScraper(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    // Set up the browser and context then scrape each URL
    public async Task StartScraping(List<UrlToScrape> urls)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            //Args = new[] { "--start-maximized" }, 
            //SlowMo = 50
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = GetRandomUserAgent(),
            Locale = "ja-JP",
            ViewportSize = new ViewportSize { Height = 1080, Width = 1920 },
        });

        await context.AddInitScriptAsync(@"Object.defineProperty(navigator, 'webdriver', {
                                        get: () => undefined,
                                        });");


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

        foreach (UrlToScrape url in urls)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
                await ScrapeSite(context, repository, url.Url);
            }
        }

        await browser.CloseAsync();
    }

    // Get a random user agent from the list
    private string GetRandomUserAgent()
    {
        Random random = new Random();
        int index = random.Next(userAgents.Count);
        return userAgents[index];
    }

    public async Task ScrapeSite(IBrowserContext context, IRepository repository, string url)
    {
        List<MercariListing> listings = new List<MercariListing>(); // Instatiate new list here so its fresh each time
        var oldListings = await repository.GetAllListings(); // Put old items into list
        int newUniqueLinks = 0; // Start the counter for new unique links
        int repeatCount = 0; // reset repeat count at the start of each scrape

        foreach (var listing in oldListings)
        {
            uniqueLinks.Add(listing.Url); // Put each old database item into the hashset
        }

        // Do the scraping
        try
        {
            var page = await context.NewPageAsync();
            await page.GotoAsync(url);

            do
            {
                Console.WriteLine("Starting the loop again! Woohoo!");
                await page.WaitForSelectorAsync("li[data-testid='item-cell']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

                // Scroll to end of page to load all listings
                await Task.Delay(new Random().Next(500, 1500));
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

                    var imgElement = await item.QuerySelectorAsync("img");
                    string? imgUrl = await imgElement?.GetAttributeAsync("src");

                    // Write to console during development
                    Console.WriteLine($"Description: {description}, Price: {price}, Link: {link}, ImgSrce: {imgUrl}");

                    // Randomly move the mouse around
                    var boundingBox = await item.BoundingBoxAsync();
                    if (boundingBox != null)
                    {
                        await page.Mouse.MoveAsync(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2);
                        await Task.Delay(new Random().Next(100, 300)); // Short pause after moving the mouse
                    }
                    await page.Mouse.MoveAsync(new Random().Next(0, 500), new Random().Next(0, 500)); // Adjust dimensions based on expected page size

                    price = price.Replace(",", "");

                    if (uniqueLinks.Contains(link))
                    {
                        repeatCount++;
                        Console.WriteLine($"Current repeats: {repeatCount}");
                        if (repeatCount >= repeatLimit) break;
                    }
                    else
                    {
                        if (int.TryParse(price, out int parsedPrice))
                        {
                            // All is normal, save the listing
                            listings.Add(new MercariListing { Description = description, Price = parsedPrice, Url = link, ImgUrl = imgUrl });
                        }
                        else
                        {
                            // Save listing with dummy price and log the error
                            listings.Add(new MercariListing { Description = description, Price = 0, Url = link, ImgUrl = imgUrl });
                            var logMessage = $"Failed to parse price for listing: {description}, Price: {price}, Url: {link}, Time: {DateTime.UtcNow}\n";
                            var logDirectory = "logs";
                            Directory.CreateDirectory(logDirectory);
                            var logFilePath = Path.Combine(logDirectory, $"failed_parses_{DateTime.UtcNow.Date:yyyyMMdd}.txt");
                            await File.AppendAllTextAsync(logFilePath, logMessage);
                        }
                        uniqueLinks.Add(link);
                        newUniqueLinks++;
                        repeatCount = 0;
                    }


                    // Random delay between actions in ms
                    await Task.Delay(new Random().Next(100, 1000));
                }

                // Go to the next page
                var nextButton = await page.QuerySelectorAsync("[data-testid='pagination-next-button']");
                if (nextButton != null && repeatCount < repeatLimit)
                {
                    await Task.Delay(new Random().Next(500, 2000));
                    await nextButton.ClickAsync();
                }
                else
                {
                    Console.WriteLine("No next button found!");
                    break;
                }

                // Keep going until you either find several repeats in a row or you've scraped the limit for that time.
            } while (repeatCount < repeatLimit && newUniqueLinks < newLinkLimit);

            await repository.AddListings(listings);
            await page.CloseAsync();
        }
        catch (Exception ex)
        {
            var logDirectory = "logs";
            var logFilePath = Path.Combine(logDirectory, "error_log.txt");
            var logMessage = $"Error occurred at {DateTime.UtcNow}: {ex.Message}\nStack Trace: {ex.StackTrace}\n";

            Directory.CreateDirectory(logDirectory);

            await File.AppendAllTextAsync(logFilePath, logMessage);

            Console.WriteLine($"An error occurred: {ex.Message}");
        }
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

    public bool CanHandleUrl(string url)
    {
        return url.Contains("jp.mercari.com");
    }
}
