using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.ReportServiceContracts;

public interface IPromptReportGetterService
{
    Task<PagedResult<PromptReportResponse>> GetPendingReportsAsync(int page = 1, int pageSize = PaginationDefaults.DefaultPageSize);
}
