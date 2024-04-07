using Microsoft.Playwright;
using System.Formats.Asn1;

var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false, SlowMo = 50 });
var page = await browser.NewPageAsync();
await page.GotoAsync("https://jp.mercari.com");

// Perform your scraping actions here
var title = await page.TitleAsync();
await page.ScreenshotAsync(new()
{
    Path = "screenshot.png"
});
Console.WriteLine($"Page Title: {title}");

// Clean up
await browser.CloseAsync();