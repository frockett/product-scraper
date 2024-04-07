

namespace product_scraper.Models;

public class FilterCriteria
{
    public int Id { get; set; }
    public List<string>? Keywords { get; set; }
    public int MinPrice { get; set; }
    public int MaxPrice { get; set; }
}
