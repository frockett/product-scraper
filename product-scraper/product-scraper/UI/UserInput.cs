using product_scraper.Models;
using product_scraper.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace product_scraper.UI;

public class UserInput
{
    public FilterCriteria? GetNewFilter()
    {
        throw new NotImplementedException();
    }

    public string? GetNewUrl()
    {
        throw new NotImplementedException();
    }

    private List<string>? GetKeywords() 
    { 
        throw new NotImplementedException(); 
    }

    private List<int>? GetPriceRange()
    {
        throw new NotImplementedException();
    }
}
