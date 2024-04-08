using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using product_scraper.Data;
using Microsoft.EntityFrameworkCore;
using product_scraper.Repositories;
using product_scraper;
using product_scraper.Services;
using product_scraper.UI;

Console.OutputEncoding = System.Text.Encoding.UTF8;



//var builder = Host.CreateApplicationBuilder();


var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<ScraperContext>(options =>
            options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));
        // Data layer stuff
        services.AddScoped<IRepository, SqliteRepository>();
        
        // Scraper factory
        services.AddSingleton<IScraperFactory, ScraperFactory>();

        // Service that manages scrapers
        services.AddScoped<ScraperService>();

        // CLI menu arguments
        services.AddScoped<Menu>();
        services.AddScoped<UserInput>();

        // Filtering and reporting
        services.AddScoped<IFilterService, FilterService>();
        services.AddScoped<EmailService>();
    });



var app = builder.Build();

/*await app.Services.GetRequiredService<ScraperService>().StartScraping();*/

// TODO decouple this from the emailservice, because currently it's all chained together.
await app.Services.GetRequiredService<IFilterService>().FilterAllUnemailedListings();

