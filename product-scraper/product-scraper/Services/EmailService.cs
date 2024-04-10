
using System.Net.Mail;
using System.Net;
using product_scraper.Dtos;
using System.Text;
using product_scraper.Models;

namespace product_scraper.Services;

public class EmailService
{
    private readonly string? emailPassword = Environment.GetEnvironmentVariable("EmailSettings__AppPassword");

    public async Task<string> GenerateEmailHtml(List<MercariListingDto> listings)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("<html>");
        sb.Append("<head>");
        sb.Append("<style>");
        sb.Append("table { width: 100%; border-collapse: collapse; }");
        sb.Append("th, td { border: 1px solid #ddd; text-align: left; padding: 8px; }");
        sb.Append("th { background-color: #f2f2f2; }");
        sb.Append("</style>");
        sb.Append("</head>");
        sb.Append("<body>");
        sb.Append("<h2>Mercari Listings</h2>");
        sb.Append("<table>");
        sb.Append("<tr><th>Description</th><th>Price</th><th>Url</th><th>Image</th></tr>");

        foreach (var listing in listings)
        {
            sb.Append("<tr>");
            sb.AppendFormat("<td>{0}</td>", listing.Description);
            sb.AppendFormat("<td>¥{0}</td>", listing.Price);
            sb.AppendFormat("<td><a href='{0}'>Link</a></td>", listing.Url);
            sb.AppendFormat("<td><img src='{0}' alt='Listing image' style='height:100px;'/></td>", listing.ImgUrl ?? "https://placeholder.com/100");
            sb.Append("</tr>");
        }

        sb.Append("</table>");
        sb.Append("</body>");
        sb.Append("</html>");

        return sb.ToString();
    }

    public async Task SendEmailAsync(string body, List<User> recipients)
    {
        var fromAddress = new MailAddress("crockett.d.ford@gmail.com");

        List<MailAddress> addresses = new();

        foreach (var recipient in recipients)
        {
            addresses.Add(new MailAddress(recipient.Email));
        }

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, emailPassword)
        };

        foreach (var address in addresses)
        {
            try
            {
                using (var message = new MailMessage(fromAddress, address)
                {
                    Subject = $"{TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")):yyyy/MM/dd HH:mm} update",
                    Body = body,
                    IsBodyHtml = true

                })
                {
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The email to {address.User} at {address.Address} could not be sent.");
                Console.WriteLine(ex.ToString());
            }
        }

    }


}
