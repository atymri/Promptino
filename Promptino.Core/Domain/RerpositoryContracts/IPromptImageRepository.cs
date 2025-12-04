using Promptino.Core.DTOs;

namespace Promptino.Core.Domain.RerpositoryContracts;

public interface IPromptImageRepository
{
    Task<PromptResponse> CreatePromptWithImagesAsync(PromptAddRequest promptRequest);
}