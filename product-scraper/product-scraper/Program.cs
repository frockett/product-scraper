using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using product_scraper.Data;
using Microsoft.EntityFrameworkCore;
using product_scraper.Repositories;
using product_scraper;
using product_scraper.Services;
using product_scraper.UI;
using Serilog;
using Serilog.Events;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.Async(a => a.File("logs/scraperlog-.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();

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
        services.AddScoped<ManagerService>();
        services.AddScoped<ScraperService>();

        // CLI menu arguments
        services.AddScoped<Menu>();
        services.AddScoped<UserInput>();

        // Filtering and reporting
        services.AddScoped<IFilterService, FilterService>();
        services.AddScoped<EmailService>();
    })
    .UseSerilog()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSerilog();
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // reduce EF core log verbosity, only show errors
        logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
    });




var app = builder.Build();

if (args.Length > 0 && args[0] == "menu")
{
    var menu = app.Services.GetRequiredService<Menu>();
    Log.Information("Menu accessed at {TimeNow}", DateTime.UtcNow);
    await menu.MainMenuAsync();
}
else
{
    Log.Information("Scraping routine started");
    await app.Services.GetRequiredService<ManagerService>().Run();
}

Log.CloseAndFlush();




