using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Services.ReportServices;
using Promptino.Core.ServiceContracts.ReportServiceContracts;

namespace Promptino.Core.Services.ReportServices;

public class PromptReportGetterService : IPromptReportGetterService
{
    private readonly IPromptReportRepository _reportRepository;

    public PromptReportGetterService(IPromptReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<PagedResult<PromptReportResponse>> GetPendingReportsAsync(int page = 1, int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = PaginationDefaults.DefaultPageSize;
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;

        var (totalCount, reports) = await _reportRepository.GetPendingPagedAsync(page, pageSize);
        return new PagedResult<PromptReportResponse>(
            reports.Select(PromptReportAdderService.ToResponse).ToList(), page, pageSize, totalCount);
    }
}
