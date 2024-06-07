using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using product_scraper.Models;
using product_scraper.Repositories;
using Serilog;

namespace product_scraper;

public class MercariScraper : IScraper
{
    //private readonly IRepository repository;
    private readonly IServiceScopeFactory scopeFactory;
    private HashSet<string> uniqueUrlHashes = new HashSet<string>();
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
    private int repeatLimit = 20;
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
            // Commented out for debugging and development
            //Headless = false,
            //Args = new[] { "--start-maximized" }, 
            //SlowMo = 50
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = GetRandomUserAgent(),
            Locale = "ja-JP",
            ViewportSize = new ViewportSize { Height = 1080, Width = 1920 },
        });

        await context.AddInitScriptAsync(@"
            // Remove WebDriver property
            delete Object.getPrototypeOf(navigator).webdriver;

            // Mock Permissions API
            const originalQuery = window.navigator.permissions.query;
            window.navigator.permissions.query = (parameters) => 
                parameters.name === 'notifications' ? 
                Promise.resolve({ state: 'denied' }) : 
                originalQuery(parameters);

            // Spoof Plugins
            Object.defineProperty(navigator, 'plugins', {
                get: () => [
                    { name: 'Chrome PDF Plugin', filename: 'internal-pdf-viewer', description: 'Portable Document Format', '__mimeTypes': ['application/pdf'] },
                    { name: 'Chrome PDF Viewer', filename: 'mhjfbmdgcfjbbpaeojofohoefgiehjai', description: '', '__mimeTypes': ['application/pdf'] },
                    { name: 'Native Client', filename: 'internal-nacl-plugin', description: '', '__mimeTypes': ['application/pdf'] },
                ]
            });

            // Spoof MIME types
            Object.defineProperty(navigator, 'mimeTypes', {
                get: () => [{
                    type: 'application/pdf',
                    suffixes: 'pdf',
                    description: '',
                    enabledPlugin: {
                        description: 'Portable Document Format',
                        filename: 'internal-pdf-viewer',
                        name: 'Chrome PDF Plugin'
                    }
                }]
            });

            // WebGL vendor and renderer spoofing
            const getParameter = WebGLRenderingContext.prototype.getParameter;
            WebGLRenderingContext.prototype.getParameter = function(parameter) {
                if (parameter === 37445) { // UNMASKED_VENDOR_WEBGL
                    return 'Intel Inc.';
                }
                if (parameter === 37446) { // UNMASKED_RENDERER_WEBGL
                    return 'Intel Iris OpenGL Engine';
                }
                return getParameter(parameter);
            };
        ");


        await context.AddCookiesAsync(new[]
        {
            new Cookie
            {
                Name = "country_code",
                Value = "JP",
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
                Log.Information("Navigating to {Url}", url.Url);
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
        List<MercariListing> listings = new();
        uniqueUrlHashes = await repository.LoadExistingUrlHashes(); // Load hashes
        int newUniqueLinks = 0; // Start the counter for new unique links
        int repeatCount = 0; // reset repeat count at the start of each scrape

        // Do the scraping
        try
        {
            var page = await context.NewPageAsync();
            await page.GotoAsync(url);

            // This code block is for debug screenshots

            //string filePath = Path.Combine(Environment.CurrentDirectory, "screenshotheadlessChromeTest.png");
            //await page.ScreenshotAsync(new PageScreenshotOptions
            //{
            //    Path = filePath
            //});

            do
            {
                Log.Information("Waiting for page load");
                await page.WaitForSelectorAsync("li[data-testid='item-cell']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

                // Scroll to end of page to load all listings
                await Task.Delay(new Random().Next(500, 1500));
                Log.Information("Began scrolling");

                try
                {
                    await ScrollToEnd(page);
                }
                catch (TimeoutException ex)
                {
                    Log.Error(ex, "An exception occurred during scrollin");
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An exception occurred during scrolling");
                    break;
                }
 
                Log.Information("Finished scrolling!");


                var items = await page.QuerySelectorAllAsync("li[data-testid='item-cell']");
                foreach (var item in items)
                {
                    // Cache all of the tags that have desired information
                    var imageContainer = await item.QuerySelectorAsync(".imageContainer__f8ddf3a2");
                    string? description = await imageContainer?.GetAttributeAsync("aria-label");

                    var priceElement = await item.QuerySelectorAsync(".number__6b270ca7");
                    string? price = await priceElement?.InnerTextAsync();

                    var linkElement = await item.QuerySelectorAsync("a[data-location='search_result:newest:body:item_list:item_thumbnail']");
                    string? link = await linkElement?.GetAttributeAsync("href");

                    var imgElement = await item.QuerySelectorAsync("img");
                    string? imgUrl = await imgElement?.GetAttributeAsync("src");

                    string? urlHash = repository.ComputeSha256Hash(link);

                    // Randomly move the mouse around
                    var boundingBox = await item.BoundingBoxAsync();
                    if (boundingBox != null)
                    {
                        await page.Mouse.MoveAsync(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2);
                        await Task.Delay(new Random().Next(100, 300)); // Short pause after moving the mouse
                    }
                    await page.Mouse.MoveAsync(new Random().Next(0, 500), new Random().Next(0, 500)); // Adjust dimensions based on expected page size

                    if(!float.TryParse(price, out float parsedFloatPrice)) // If it can't be parsed as a float, then carry on as normal (maybe it's in JPY again)
                    {
                        price = price.Replace(",", "");
                    }
                    else
                    {
                        price = Convert.ToInt32(parsedFloatPrice).ToString(); // This is horrifically stupid but I don't want to refactor the int parsing code right now, this is a band-aid  fix for when the price is displayed in SGD
                    }
                    

                    if (uniqueUrlHashes.Contains(urlHash))
                    {
                        repeatCount++;
                        if (repeatCount >= repeatLimit) break;
                    }
                    else
                    {
                        if (int.TryParse(price, out int parsedPrice))
                        {
                            // All is normal, save the listing
                            listings.Add(new MercariListing { Description = description, Price = parsedPrice, Url = link, UrlHash = urlHash, ImgUrl = imgUrl });
                        }
                        else
                        {
                            // Save listing with dummy price and log the error
                            listings.Add(new MercariListing { Description = description, Price = 0, Url = link, UrlHash = urlHash, ImgUrl = imgUrl });
                            Log.Error("Failed to parse price for listing: {description}, Price: {price}, Url: {link}, Time: {DateTime}", description, price, link, DateTime.UtcNow);
                        }
                        uniqueUrlHashes.Add(urlHash);
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
                else if (nextButton == null)
                {
                    Log.Error("No next button found!");
                    break;
                }
                else if (repeatCount >= repeatLimit)
                {
                    Log.Information("Repeat limit of {RepeatLimit} has been met!", repeatLimit);
                    break;
                }

                // Keep going until you either find several repeats in a row or you've scraped the limit for that time.
            } while (repeatCount < repeatLimit && newUniqueLinks < newLinkLimit);

            await repository.AddListings(listings);
            await page.CloseAsync();
            Log.Information("{listingsCount} listings saved from {url}.", listings.Count, url);
        }
        catch (Exception ex)
        {
            Log.Error("Error occurred while operating MercariScraper: {message}. {stackTrace}", ex.Message, ex.StackTrace);
        }
    }

    private async Task ScrollToEnd(IPage page)
    {
        try
        {
            var jsScript = @"async () => {
                return new Promise((resolve, reject) => {
                    var totalHeight = 0;
                    var distance = 100;
                    var scrollUpEvery = 5; // Scroll up every 5 scrolls
                    var scrollCount = 0;
                    var timer = setInterval(() => {
                        var randDistance = distance + Math.floor(Math.random() * 100) - 50;
                        if (scrollCount % scrollUpEvery === 4) {
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
            }"; 

            var scrollingTask = page.EvaluateAsync(jsScript);
            if (await Task.WhenAny(scrollingTask, Task.Delay(120000)) == scrollingTask)
            {
                await scrollingTask;
            }
            else
            {
                Log.Information("Scrolling operation timed out.");
                throw new TimeoutException("The scrolling operation timed out.");
            }
        }
        catch (TimeoutException ex)
        {
            // Pass the exception up to the call site
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("Exception during scrolling: {exMessage}", ex.Message);
            throw;
        }

    }

    public bool CanHandleUrl(string url)
    {
        return url.Contains("jp.mercari.com");
    }
}
