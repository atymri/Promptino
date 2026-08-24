using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IPromptUpdaterService
{
    Task<PromptResponse?> UpdatePromptAsync(PromptUpdateRequest promptRequest, Guid currentUserId, bool isAdmin);
}
