using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.SavedPromptServiceContracts;

public interface ISavedPromptGetterService
{
    Task<IEnumerable<SavedWithDetailsResponse>> GetSavedPromptsAsync(Guid userId);
    Task<int> GetSavedCountAsync(Guid promptId);
    Task<bool> IsSavedAsync(Guid userId, Guid promptId);
}
