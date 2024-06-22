using product_scraper.Models;
using product_scraper.Repositories;
using Spectre.Console;
using Serilog;

namespace product_scraper.UI;

public class Menu
{
    private readonly UserInput userInput;
    private readonly IRepository repository;

    public Menu(UserInput userInput, IRepository repository)
    {
        this.userInput = userInput;
        this.repository = repository;
    }

    public async Task MainMenuAsync()
    {
        AnsiConsole.Clear();

        string[] menuOptions =
                {"Add Filter",
                "Delete Filter",
                "In/activate URLs",
                "Add URL",
                "Reset all emailed flags CAUTION",
                "Update Url Hashes CAUTION",
                "Exit Program",};

        string choice = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                            .Title("Which operation would you like to perform?")
                            .AddChoices(menuOptions));

        /* Before, the menu selection was parsed based on an int.parse of the first character, which was a number. 
        *  But having the numbers could confuse the user, since you can't input a number in the menu.
        *  So instead, menuSelection is the index in the menu array + 1 (the +1 is for ease of human readability) */

        int menuSelection = Array.IndexOf(menuOptions, choice) + 1;

        switch (menuSelection)
        {
            case 1:
                await HandleAddFilterAsync();
                break;
            case 2:
                await HandleDeleteFilter();
                break;
            case 3:
                await HandleUrlMenu();
                break;
            case 4:
                await HandleAddUrl();
                break;
            case 5:
                await HandleResetFlags();
                break;
            case 6:
                await HandleUpdateHashes();
                break;
            case 7:
                Environment.Exit(0);
                break;
        }
    }

    private async Task HandleUpdateHashes()
    {
        await repository.UpdateUrlHashes();
        await WaitForUser();
    }

    private async Task HandleResetFlags()
    {
        await repository.ResetEmailFlags();
        await WaitForUser();
    }

    private async Task HandleAddFilterAsync()
    {
        FilterCriteria? newFilter = userInput.GetNewFilter();
        if (newFilter != null)
        {
            await repository.AddFilter(newFilter);
        }
        else
        {
            await WaitForUser();
        }
        await WaitForUser();
    }

    private async Task HandleDeleteFilter()
    {
        var filters = await repository.GetAllFilterCriteria();
        FilterCriteria filterToDelete = userInput.GetFilterToDelete(filters);

        bool deleteConfired = AnsiConsole.Confirm($"Are you sure you want to delete filter with keywords {String.Join(", ", filterToDelete.Keywords)} and price range {filterToDelete.MinPrice} - {filterToDelete.MaxPrice}?");

        if (deleteConfired)
        {
            try
            {
                await repository.DeleteFilter(filterToDelete);
                AnsiConsole.WriteLine("Filter deleted!");
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"An error occured deleting the filter: {ex.Message}");
            }
        }
        else
        {
            AnsiConsole.WriteLine("Okay! Filter will not be deleted");
        }

        await WaitForUser();
    }

    private async Task WaitForUser()
    {
        AnsiConsole.WriteLine("Press enter to return to main menu...");
        Console.ReadLine();
        await MainMenuAsync();
    }

    private async Task HandleUrlMenu()
    {
        var urls = await repository.GetAllUrls();
        UrlToScrape selectedUrl = userInput.GetSelectedUrl(urls);

        AnsiConsole.Clear();

        AnsiConsole.WriteLine($"{selectedUrl.Url} is currently active: {selectedUrl.Active}");

        string[] menuOptions =
                {"Toggle active",
                "Exit",};

        string choice = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                            .Title("Which operation would you like to perform? Use [green]arrow[/] and [green]enter[/] keys to make a selection.")
                            .PageSize(10)
                            .MoreChoicesText("Keep scrolling for more options")
                            .AddChoices(menuOptions));

        int menuSelection = Array.IndexOf(menuOptions, choice) + 1;

        switch (menuSelection)
        {
            case 1:
                await repository.ToggleUrlActiveStatus(selectedUrl.Id);
                AnsiConsole.WriteLine($"URL {selectedUrl.Url} is now active: {selectedUrl.Active}");
                await WaitForUser();
                break;
            case 2:
                await WaitForUser();
                break;
        }
    }

    private async Task HandleAddUrl()
    {
        UrlToScrape? urlToAdd = userInput.GetNewUrl();

        try
        {
            if (urlToAdd != null)
            {
                await repository.AddUrl(urlToAdd);
            }
            else
            {
                await WaitForUser();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
        await WaitForUser();
    }
}
