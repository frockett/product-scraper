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
        await scraperService.StartScraping();
        var flaggedListings = await filterService.FilterAllUnemailedListings();
        string emailBody = await emailService.GenerateEmailHtml(flaggedListings);
        List<User>? users = await repository.GetAllUsers();
        if (users != null && users.Count != 0)
        {
            await emailService.SendEmailAsync(emailBody, users);
        }
        else return;
    }
}
