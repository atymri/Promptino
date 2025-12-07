using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;

namespace Promptino.Core.Services.PromptServices;

public class PromptDeleterService : IPromptDeleterService
{
    private readonly IPromptRepository _promptReposiotry;
    public PromptDeleterService(IPromptRepository promptReposiotry)
    {
        _promptReposiotry = promptReposiotry;
    }

    public async Task<bool> DeletePromptAsync(Guid id)
    {
        if (!await _promptReposiotry.DoesPromptExistAsync(id))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر وجود ندارد");

        return await _promptReposiotry.DeletePromptAsync(id);
    }

    public async Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid promptId)
    {
        var isFavorite = await _promptReposiotry.IsFavoriteAsync(userId, promptId);
        if (!isFavorite)
            throw new PromptNotFoundExceptions("پرامپت مورد نظر در مورد علاقه ها وجود ندارد");

        return await _promptReposiotry.RemoveFromFavoritesAsync(userId, promptId);
    }
}