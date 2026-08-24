namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IPromptDeleterService
{
    Task<bool> DeletePromptAsync(Guid id, Guid currentUserId, bool isAdmin);
}
