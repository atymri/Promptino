using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.SavedPromptServiceContracts;

public interface ISavedPromptAdderService
{
    Task<SavedWithDetailsResponse> SaveAsync(Guid userId, Guid promptId);
}
