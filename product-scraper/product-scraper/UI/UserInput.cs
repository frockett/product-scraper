using product_scraper.Models;
using product_scraper.Repositories;
using Spectre.Console;


namespace product_scraper.UI;

public class UserInput
{
    public FilterCriteria? GetNewFilter()
    {
        AnsiConsole.Clear();

        List<string> newKeywords = new();
        
        string? keyword = AnsiConsole.Ask<string?>("Enter as many keywords as you like. Leave blank and hit enter to finish: ", null);
        
        if (keyword != null)
        {
            newKeywords.Add(keyword);
        }
        else
        {
            return null;
        }

        while (true)
        {
            keyword = AnsiConsole.Ask<string?>("Enter as many keywords as you like. Leave blank and hit enter to finish: ", null);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                break;
            }
            newKeywords.Add(keyword);
        }

        int minPrice = AnsiConsole.Ask<int>($"Enter minumum price for keywords '{String.Join(", ", newKeywords)}': ");
        int maxPrice = AnsiConsole.Ask<int>($"Enter max price for keywords '{String.Join(", ", newKeywords)}': ");

        while (minPrice > maxPrice)
        {
            AnsiConsole.WriteLine("INVALID: make sure min price is lower than max");
            minPrice = AnsiConsole.Ask<int>($"Enter minumum price for keywords '{String.Join(", ", newKeywords)}': ");
            maxPrice = AnsiConsole.Ask<int>($"Enter max price for keywords '{String.Join(", ", newKeywords)}': ");
        }

        return new FilterCriteria
        {
            Keywords = newKeywords,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };
    }

    public UrlToScrape? GetNewUrl()
    {
        string input = AnsiConsole.Ask<string>("paste new URL: ");

        while (!IsValidUrl(input))
        {
            input = AnsiConsole.Ask<string>("paste valid URL: ");
        }

        return new UrlToScrape
        {
            Url = input
        };
    }

    public FilterCriteria GetFilterToDelete(List<FilterCriteria> currentFilters)
    {
        AnsiConsole.Clear();
        var options = new SelectionPrompt<FilterCriteria>();
        options.AddChoices(currentFilters);
        options.UseConverter(currentFilters => $"{String.Join(", ", currentFilters.Keywords)} -- Price range: {currentFilters.MinPrice} - {currentFilters.MaxPrice}");

        FilterCriteria filterToDelete = AnsiConsole.Prompt(options);
        return filterToDelete;
    }

    public UrlToScrape GetSelectedUrl(List<UrlToScrape> urls)
    {
        AnsiConsole.Clear();
        var options = new SelectionPrompt<UrlToScrape>();
        options.AddChoices(urls);
        options.UseConverter(urls => $"{urls.Url} || Active: {urls.Active}");

        UrlToScrape selectedUrl = AnsiConsole.Prompt(options);
        return selectedUrl;
    }

    private bool IsValidUrl(string url)
    {
        bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        return result;
    }

}
