using product_scraper.Dtos;
using product_scraper.Models;
using product_scraper.Repositories;

namespace product_scraper.Services;

public class ManagerService
{
    private readonly IRepository repository;
    private readonly EmailService emailService;
    private readonly IFilterService filterService;
    private readonly ScraperService scraperService;

    public ManagerService(IRepository repository, EmailService emailService, IFilterService filterService, ScraperService scraperService)
    {
        this.repository = repository;
        this.emailService = emailService;
        this.filterService = filterService;
        this.scraperService = scraperService;
    }

    public async Task Run()
    {
        try
        {
            await scraperService.StartScraping();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during scraping: {ex.Message} \n {ex.StackTrace}");
            return;
        }

        List<MercariListingDto> flaggedListings = new();

        try
        {
            flaggedListings = await filterService.FilterAllUnemailedListings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during filtering: {ex.Message} \n {ex.StackTrace}");
        }

        try
        {
            string emailBody = await emailService.GenerateEmailHtml(flaggedListings);
            List<User>? users = await repository.GetAllUsers();
            if (users != null && users.Count != 0)
            {
                await emailService.SendEmailAsync(emailBody, users);
            }
            else return;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error during email creation/sending: {ex.Message} \n {ex.StackTrace}");
        }

        try
        {
            await repository.DeleteOldListings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during DB clean-up: {ex.Message} \n {ex.StackTrace}");
        }
    }
}
