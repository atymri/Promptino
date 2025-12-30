using System.Net;
using System.Net.Mail;
using Promptino.API.Models;

namespace Promptino.API.Tools;

public static class EmailSender
{
    public static async Task SendAsync(EmailModel emailModel)
    {
        var address = "amirmahditeymoori123@gmail.com";
        var message = new MailMessage()
        {
            From = new MailAddress(address, "پرامپتینو"),
            To = { emailModel.To },
            Subject = emailModel.Subject,
            Body = emailModel.Body,
            IsBodyHtml = true
        };

        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(address, "abak aape zjrg shvr"),
            EnableSsl = true
        };

        client.Send(message);
    }
}