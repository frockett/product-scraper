using Microsoft.Playwright;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using product_scraper.Data;
using Microsoft.EntityFrameworkCore;
using product_scraper.Repositories;
using product_scraper;

Console.OutputEncoding = System.Text.Encoding.UTF8;



var builder = Host.CreateApplicationBuilder();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

builder.Services.AddDbContext<ScraperContext>(options =>
{
    options.UseSqlite(config.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IRepository, SqliteRepository>();
builder.Services.AddScoped<ScraperService>();


var app = builder.Build();

var scope = app.Services.CreateScope();

var scraper = app.Services.GetRequiredService<ScraperService>();

await scraper.ScrapeSite();