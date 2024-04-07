using Microsoft.Playwright;

Console.OutputEncoding = System.Text.Encoding.UTF8;

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
        Value = "TW", // Setting to Japan
        Domain = "jp.mercari.com",
        Path = "/",
        Expires = DateTimeOffset.Now.AddDays(30).ToUnixTimeSeconds() // Ensure the cookie is valid for some time
    }
});

var page = await context.NewPageAsync();
await page.GotoAsync("https://jp.mercari.com/search?order=desc&sort=created_time&category_id=20");



await page.WaitForSelectorAsync("li[data-testid='item-cell']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

await ScrollToEnd(page);
// Now proceed to extract data from the li tags


var items = await page.QuerySelectorAllAsync("li[data-testid='item-cell']");
foreach (var item in items)
{
    var imageContainer = await item.QuerySelectorAsync(".imageContainer__f8ddf3a2");
    string? description = await imageContainer?.GetAttributeAsync("aria-label");

    var priceElement = await item.QuerySelectorAsync(".number__6b270ca7");
    string? price = await priceElement?.InnerTextAsync();

    var linkElement = await item.QuerySelectorAsync("a[data-location='search_result:newest:body:item_list:item_thumbnail']");
    string? link = await linkElement?.GetAttributeAsync("href");

    Console.WriteLine($"Description: {description}, Price: {price}, Link: {link}");
}

// Clean up
await browser.CloseAsync();



async Task ScrollToEnd(IPage page)
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
