using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.PromptReactionServiceContracts;

namespace Promptino.Core.Services.PromptReactionServices;

public class PromptReactionSetterService : IPromptReactionSetterService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IPromptReactionRepository _reactionRepository;
    private readonly IMapper _mapper;

    public PromptReactionSetterService(
        IPromptRepository promptRepository,
        IPromptReactionRepository reactionRepository,
        IMapper mapper)
    {
        _promptRepository = promptRepository;
        _reactionRepository = reactionRepository;
        _mapper = mapper;
    }

    public async Task<ReactionStateResponse> SetReactionAsync(Guid userId, Guid promptId, ReactionType reaction)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(userId));

        if (!await _promptRepository.DoesPromptExistAsync(promptId))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر پیدا نشد");

        var existing = await _reactionRepository.GetReactionAsync(userId, promptId);

        if (existing is null)
        {
            await _reactionRepository.AddReactionAsync(
                new PromptReaction { UserID = userId, PromptID = promptId, Type = reaction });
        }
        else if (existing.Type == reaction)
        {
            // clicking the same button again un-toggles it
            await _reactionRepository.RemoveReactionAsync(userId, promptId);
        }
        else
        {
            existing.Type = reaction;
            existing.Touch();
            await _reactionRepository.UpdateReactionAsync(existing);
        }

        var counts = await _reactionRepository.GetCountsAsync(promptId);
        var myReaction = await _reactionRepository.GetReactionAsync(userId, promptId);

        return new ReactionStateResponse(counts.Likes, counts.Dislikes, myReaction?.Type);
    }
}
