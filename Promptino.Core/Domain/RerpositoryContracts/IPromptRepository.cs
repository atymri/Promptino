using Promptino.Core.Domain.Entities;
using System.Linq.Expressions;

namespace Promptino.Core.Domain.RepositoryContracts;

public interface IPromptRepository
{
    Task<IEnumerable<Prompt>> GetPromptsAsync();
    Task<IEnumerable<Prompt>> SearchPromptAsync(string keyword);

    Task<IEnumerable<Prompt>> GetPromptsByConditionAsync(Expression<Func<Prompt, bool>> condition);
    Task<Prompt?> GetPromptByConditionAsync(Expression<Func<Prompt, bool>> condition);

    Task<Prompt?> AddPromptAsync(Prompt prompt);
    Task<Prompt?> UpdatePromptAsync(Prompt prompt);
    Task<bool> DeletePromptAsync(Guid id);

    Task<IEnumerable<Prompt>> GetFavoritePromptsAsync(Guid userId);
    Task<bool> IsFavoriteAsync(Guid userId, Guid promptId);
    Task AddToFavoritesAsync(Guid userId, Guid promptId);
    Task RemoveFromFavoritesAsync(Guid userId, Guid promptId);
}
