using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts;
public interface IEmailService
{
    Task SendAsync(EmailModel emailModel);
    Task<string> ConvertHtmlAsync(HtmlConvertorModel convertorModel); 
}
