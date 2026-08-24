using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Domain.RepositoryContracts;

public interface ISavedPromptRepository
{
    Task<IEnumerable<SavedPrompt>> GetSavedByUserAsync(Guid userId);
    Task<bool> IsSavedAsync(Guid userId, Guid promptId);
    Task<bool> AddSavedPromptAsync(SavedPrompt saved);
    Task<bool> RemoveSavedPromptAsync(Guid userId, Guid promptId);
    Task<int> GetSavedCountAsync(Guid promptId);
}
