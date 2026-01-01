using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Promptino.Core.DTOs;
using Promptino.Core.Options;
using Promptino.Core.ServiceContracts;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Promptino.Core.Services;

internal class EmailService : IEmailService
{
    private readonly IWebHostEnvironment _env;
    private readonly EmailCredentials _creds;

    public EmailService(IWebHostEnvironment env,
        IOptions<EmailCredentials> creds)
    {
        _env = env;
        _creds = creds.Value;
    }

    public async Task<string> ConvertHtmlAsync(HtmlConvertorModel convertorModel)
    {

        var path = Path.Combine(
            _env.ContentRootPath,
            "EmailTemplates",
            convertorModel.FileName
            );

        var html = await File.ReadAllTextAsync(path, Encoding.UTF8);

        foreach(var item in convertorModel.Values)
        {
            html = html.Replace($"{{{{{item.Key}}}}}", item.Value);
        }

        return html;
    }

    public void Send(EmailModel emailModel)
    {
        var message = new MailMessage()
        {
            From = new MailAddress(_creds.MailAddress, _creds.MailTitle),
            To = { emailModel.To },
            Subject = emailModel.Subject,
            Body = emailModel.Body,
            IsBodyHtml = true
        };

        var client = new SmtpClient(_creds.StmlHost, _creds.StmpPort)
        {
            Credentials = new NetworkCredential(_creds.MailAddress, _creds.StmpPassword),
            EnableSsl = true
        };

        client.Send(message);
    }
}
