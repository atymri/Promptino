using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts;
public interface IEmailService
{
    void Send(EmailModel emailModel);
    Task<string> ConvertHtmlAsync(HtmlConvertorModel convertorModel); 
}
