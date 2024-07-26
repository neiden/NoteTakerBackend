
namespace Token.Services;

using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService
{

    private readonly IConfiguration _configuration;
    private string apiKey = "";

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        apiKey = _configuration["SENDGRID_API_KEY"]!;
    }

    public async Task<bool> SendEmail(EmailAddress to, string content, string htmlContent, string subject)
    {
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("hello@fluentflow.net", "FluentFlow");
        var msg = MailHelper.CreateSingleEmail(from, to, subject, content, htmlContent);
        var response = await client.SendEmailAsync(msg);
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            return true;
        }
        return false;
    }


}