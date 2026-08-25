using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.ReportServiceContracts;

public interface IPromptReportAdderService
{
    Task<PromptReportResponse> AddReportAsync(Guid reporterId, PromptReportAddRequest request);
}
