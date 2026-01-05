using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using Promptino.Core.DTOs;
using Promptino.Core.Options;
using Promptino.Core.ServiceContracts;
using System.Text;

namespace Promptino.Core.Services;

internal class EmailService : IEmailService
{
    private readonly IWebHostEnvironment _env;
    private readonly EmailCredentials _creds;

    public EmailService(
        IWebHostEnvironment env,
        IOptionsMonitor<EmailCredentials> creds)
    {
        _env = env;
        _creds = creds.CurrentValue;
    }

    public async Task<string> ConvertHtmlAsync(HtmlConvertorModel convertorModel)
    {
        var path = Path.Combine(
            _env.ContentRootPath,
            "EmailTemplates",
            convertorModel.FileName
        );

        var html = await File.ReadAllTextAsync(path, Encoding.UTF8);

        foreach (var item in convertorModel.Values)
        {
            html = html.Replace($"{{{{{item.Key}}}}}", item.Value);
        }

        return html;
    }

    public async Task SendAsync(EmailModel emailModel)
    {
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(_creds.MailTitle, _creds.MailAddress)
        );

        message.To.Add(
            MailboxAddress.Parse(emailModel.To)
        );

        message.Subject = emailModel.Subject;

        message.Body = new TextPart("html")
        {
            Text = emailModel.Body
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _creds.StmlHost,
            _creds.StmpPort,
            SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            _creds.MailAddress,
            _creds.StmpPassword.Replace(" ", "")
        );

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
