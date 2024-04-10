using product_scraper.Dtos;
using product_scraper.Models;
using product_scraper.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
