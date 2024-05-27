using product_scraper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace product_scraper.Services;

public static class CurrencyService
{
    public static async Task<List<MercariListingDto>> FetchSGDToCNY(List<MercariListingDto> flaggedListings)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/sgd.json";

            /* This code block fetches the current exchange rate only for SGD to CNY. Future changes could make it more extensible,
             * for example by reading the user preference from a list in the db */
            try
            {
                string response = await client.GetStringAsync(url);

                if (response != null)
                {
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        JsonElement root = doc.RootElement;
                        if (root.TryGetProperty("sgd", out JsonElement sgdRoot))
                        {
                            if (sgdRoot.TryGetProperty("cny", out JsonElement cnyElement))
                            {
                                double cnyValue = cnyElement.GetDouble();

                                foreach (var listing in flaggedListings)
                                {
                                    listing.Price = Convert.ToInt32(listing.Price * cnyValue); 
                                }
                            }
                        }
                    }
                }

                return flaggedListings;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return flaggedListings;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error parsing JSON: {e.Message}");
                return flaggedListings;
            }
        }
    }

}
