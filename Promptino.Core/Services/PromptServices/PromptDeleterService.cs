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

    public async Task<bool> DeletePromptAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        if (!await _promptReposiotry.DoesPromptExistAsync(id))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر وجود ندارد");

        var ownerId = await _promptReposiotry.GetPromptOwnerIdAsync(id);
        if (!isAdmin && ownerId != currentUserId)
            throw new PromptOwnershipException("شما اجازه حذف این پرامپت را ندارید");

        return await _promptReposiotry.DeletePromptAsync(id);
    }
}
