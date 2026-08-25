using Promptino.Core.Domain.Entities;
using System.Linq.Expressions;

namespace Promptino.Core.Domain.RepositoryContracts;

public interface IPromptRepository
{
    Task<(int TotalCount, IReadOnlyList<Prompt> Items)> GetPromptsPagedAsync(int page, int pageSize);
    Task<IReadOnlyList<Prompt>> GetPromptsByCursorAsync(DateTime? cursorCreatedAt, Guid? cursorId, int pageSize);
    Task<(int TotalCount, IReadOnlyList<Prompt> Items)> SearchPromptPagedAsync(string keyword, int page, int pageSize);

    Task<IEnumerable<Prompt>> GetPromptsByConditionAsync(Expression<Func<Prompt, bool>> condition);
    Task<Prompt?> GetPromptByConditionAsync(Expression<Func<Prompt, bool>> condition);

    Task<IEnumerable<Prompt>> GetPromptsByOwnerAsync(Guid userId);
    Task<Guid?> GetPromptOwnerIdAsync(Guid promptId);

    Task<Prompt?> AddPromptAsync(Prompt prompt);
    Task<Prompt?> UpdatePromptAsync(Prompt prompt);
    Task<bool> DeletePromptAsync(Guid id);

    Task<bool> DoesPromptExistAsync(Guid promptId);
    Task<bool> UpdateHiddenFlagAsync(Guid promptId, bool isHidden);
}
