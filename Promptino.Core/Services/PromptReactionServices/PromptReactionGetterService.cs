using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.PromptReactionServiceContracts;

namespace Promptino.Core.Services.PromptReactionServices;

public class PromptReactionGetterService : IPromptReactionGetterService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IPromptReactionRepository _reactionRepository;

    public PromptReactionGetterService(
        IPromptRepository promptRepository,
        IPromptReactionRepository reactionRepository)
    {
        _promptRepository = promptRepository;
        _reactionRepository = reactionRepository;
    }

    public async Task<ReactionStateResponse> GetStateAsync(Guid userId, Guid promptId)
    {
        if (!await _promptRepository.DoesPromptExistAsync(promptId))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر پیدا نشد");

        var counts = await _reactionRepository.GetCountsAsync(promptId);
        var existing = userId == Guid.Empty
            ? null
            : await _reactionRepository.GetReactionAsync(userId, promptId);

        return new ReactionStateResponse(counts.Likes, counts.Dislikes, existing?.Type);
    }
}
