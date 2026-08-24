using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.SavedPromptServiceContracts;

namespace Promptino.Core.Services.SavedPromptServices;

public class SavedPromptDeleterService : ISavedPromptDeleterService
{
    private readonly ISavedPromptRepository _savedPromptRepository;

    public SavedPromptDeleterService(ISavedPromptRepository savedPromptRepository)
    {
        _savedPromptRepository = savedPromptRepository;
    }

    public async Task<bool> UnsaveAsync(Guid userId, Guid promptId)
    {
        if (!await _savedPromptRepository.IsSavedAsync(userId, promptId))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر در ذخیره‌شده‌ها وجود ندارد");

        return await _savedPromptRepository.RemoveSavedPromptAsync(userId, promptId);
    }
}
