using Microsoft.Playwright;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using product_scraper.Data;
using Microsoft.EntityFrameworkCore;
using product_scraper.Repositories;
using product_scraper;
using product_scraper.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;



//var builder = Host.CreateApplicationBuilder();


var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<ScraperContext>(options =>
            options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IRepository, SqliteRepository>();
        services.AddScoped<ScraperService>();
        services.AddScoped<FilterService>();
    });



var app = builder.Build();


await app.Services.GetRequiredService<FilterService>().FilterNewListings();

/*await app.Services.GetRequiredService<ScraperService>().StartScraping();*/
