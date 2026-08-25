using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IPromptGetterService
{
    Task<PagedResult<PromptResponse>> GetAllPromptsAsync(int page = 1, int pageSize = PaginationDefaults.DefaultPageSize);
    Task<CursorResult<PromptResponse>> GetFeedByCursorAsync(string? cursor = null, int pageSize = PaginationDefaults.DefaultPageSize);
    Task<PagedResult<PromptResponse>> SearchPromptsAsync(string keyword, int page = 1, int pageSize = PaginationDefaults.DefaultPageSize);
    Task<IEnumerable<PromptResponse>> GetPromptsByConditionAsync(Expression<Func<PromptResponse, bool>> condition);
    Task<PromptResponse> GetPromptByConditionAsync(Expression<Func<PromptResponse, bool>> condition);
    Task<IEnumerable<PromptResponse>> GetPromptsByOwnerAsync(Guid userId);
    Task<IReadOnlyList<PromptVersionResponse>> GetVersionsAsync(Guid promptId);
    Task<PromptVersionResponse?> GetVersionAsync(Guid promptId, int versionNumber);
}
