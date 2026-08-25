using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Domain.RepositoryContracts;

public interface IPromptReportRepository
{
    Task<PromptReport?> GetByIdAsync(Guid reportId);
    Task<bool> HasPendingReportAsync(Guid reporterId, Guid promptId);
    Task<PromptReport> AddAsync(PromptReport report);
    Task<(int TotalCount, IReadOnlyList<PromptReport> Items)> GetPendingPagedAsync(int page, int pageSize);
    Task<bool> SaveAsync();
}
