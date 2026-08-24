namespace Promptino.Core.ServiceContracts.SavedPromptServiceContracts;

public interface ISavedPromptDeleterService
{
    Task<bool> UnsaveAsync(Guid userId, Guid promptId);
}
