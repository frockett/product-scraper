using product_scraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace product_scraper.Dtos;

public class MercariListingDto
{
    public required string Description { get; set; }
    public int Price { get; set; }
    public required string Url { get; set; }
    public string? ImgUrl { get; set; }
    public required FilterCriteria Filter { get; set; }
}
