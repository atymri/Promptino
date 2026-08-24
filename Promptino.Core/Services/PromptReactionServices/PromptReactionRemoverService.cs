using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.PromptReactionServiceContracts;

namespace Promptino.Core.Services.PromptReactionServices;

public class PromptReactionRemoverService : IPromptReactionRemoverService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IPromptReactionRepository _reactionRepository;

    public PromptReactionRemoverService(
        IPromptRepository promptRepository,
        IPromptReactionRepository reactionRepository)
    {
        _promptRepository = promptRepository;
        _reactionRepository = reactionRepository;
    }

    public async Task<ReactionStateResponse> RemoveReactionAsync(Guid userId, Guid promptId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(userId));

        if (!await _promptRepository.DoesPromptExistAsync(promptId))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر پیدا نشد");

        await _reactionRepository.RemoveReactionAsync(userId, promptId);

        var counts = await _reactionRepository.GetCountsAsync(promptId);
        return new ReactionStateResponse(counts.Likes, counts.Dislikes, null);
    }
}
