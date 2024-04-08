using Spectre.Console;

namespace product_scraper.UI;

public class Menu
{
    public void MainMenu()
    {
        AnsiConsole.Clear();

        string[] menuOptions =
                {"Add Filter",
                "Delete Filter",
                "In/activate URLs",
                "Add URL",
                "Exit Program",};

        string choice = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                            .Title("Which operation would you like to perform? Use [green]arrow[/] and [green]enter[/] keys to make a selection.")
                            .PageSize(10)
                            .MoreChoicesText("Keep scrolling for more options")
                            .AddChoices(menuOptions));

        /* Before, the menu selection was parsed based on an int.parse of the first character, which was a number. 
        *  But having the numbers could confuse the user, since you can't input a number in the menu.
        *  So instead, menuSelection is the index in the menu array + 1 (the +1 is for ease of human readability) */

        int menuSelection = Array.IndexOf(menuOptions, choice) + 1;

        switch (menuSelection)
        {
            case 1:
                HandleAddFilter();
                break;
            case 2:
                HandleDeleteFilter();
                break;
            case 3:
                HandleUrlMenu();
                break;
            case 4:
                HandleAddUrl();
                break;
            case 5:
                Environment.Exit(0);
                break;
        }
    }

    private void HandleAddFilter()
    {
        throw new NotImplementedException();
    }

    private void HandleDeleteFilter()
    {
        throw new NotImplementedException();
    }

    private void HandleUrlMenu()
    {
        throw new NotImplementedException();
    }

    private void HandleAddUrl()
    {
        throw new NotImplementedException();
    }
}
