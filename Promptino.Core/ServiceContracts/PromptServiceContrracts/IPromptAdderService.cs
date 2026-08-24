using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IPromptAdderService
{
    Task<PromptResponse> CreatePromptAsync(PromptAddRequest promptRequest, Guid ownerId);
}
